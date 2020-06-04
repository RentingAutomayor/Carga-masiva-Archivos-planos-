using System;
using System.Linq;
using DAO;
using BusinessObjects;
using System.Collections.Generic;
using System.Configuration;

namespace CargaMasiva
{
		class Program
		{
				static void Main(string[] args)
				{

						String sline = "------------------------------------------------------------------------------";
						List<String> lsData = new List<string>();
						BatchProcesator bp = new BatchProcesator();
						ProgramConfig configuration = new ProgramConfig();
						configuration.pathOrigin = ConfigurationManager.AppSettings["PathOrigin"];
						configuration.separator = ConfigurationManager.AppSettings["Separator"];
						configuration.kindOfChargue = ConfigurationManager.AppSettings["KindOfChargue"];
						configuration.pathError = ConfigurationManager.AppSettings["PathError"];
						configuration.nameFileError = ConfigurationManager.AppSettings["NameFileError"];

						lsData = bp.readFile(@configuration.pathOrigin);

						switch (configuration.kindOfChargue) {
								case "Clientes":
										List<ClientViewModel> lsClients = new List<ClientViewModel>();
										lsClients = bp.validateDataClient(lsData, configuration.separator);
										bp.createClients(lsClients);
										break;
								case "Contactos":
										List<ContactViewModel> lsContact = new List<ContactViewModel>();
										lsContact = bp.validateDataContact(lsData, configuration.separator);
										string headers = lsData[0];
										headers += "\n";
										List<string> lsDataError = new List<string>(); ;
										bp.createContacts(lsContact,configuration.separator,headers,out lsDataError);

									  if (lsDataError.Count() > 1) {
												bp.createErrorFile(lsDataError, "contacts",configuration);
										}
										break;
								case "Solicitudes":
										List<RequestViewModel> lsRequest = new List<RequestViewModel>();
										lsRequest = bp.validateDataRequests(lsData, configuration.separator);
										List<string> lsDataErrorRqt = new List<string>();
										string headersRqt = lsData[0];
										headersRqt += "\n";
										bp.CreateRequests(lsRequest,configuration.separator, headersRqt,out lsDataErrorRqt);
										if (lsDataErrorRqt.Count() > 1)
										{
												bp.createErrorFile(lsDataErrorRqt, "Requests", configuration);
										}

										Console.WriteLine("Finaliza validación de solicitudes");
										break;
								default:
										Console.WriteLine(sline);
										break;
						}				

				}
		}
}
