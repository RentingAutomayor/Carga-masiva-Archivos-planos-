using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using BusinessObjects;
using DAO;

namespace CargaMasiva
{
		class BatchProcesator
		{
				public List<String> readFile(String filePath)
				{
						try
						{
								using (StreamReader sr = new StreamReader(filePath, Encoding.UTF8, true))
								{
										List<string> lsData = new List<string>();

										while (!sr.EndOfStream)
										{
												var line = sr.ReadLine();

												lsData.Add(line);
										}

										return lsData;

								}
						}
						catch (Exception ex)
						{
								Console.Write(ex.Message);
								throw ex;
						}
				}

				public List<ClientViewModel> validateDataClient(List<string> lsData, string separator)
				{
						String sline = "--------------------------------------------------------------------------";
						List<ClientViewModel> lsClient = new List<ClientViewModel>();

						using (BDRAEntities db = new BDRAEntities())
						{
								int cont = 0;
								Console.WriteLine(sline);
								Console.WriteLine("[Inicia evaluación de clientes]");
								Console.WriteLine(sline);

								foreach (var line in lsData)
								{
										cont += 1;

										var aData = line.Split(separator);
										ClientViewModel cli = new ClientViewModel();
										KindOfDocumentViewModel kod = new KindOfDocumentViewModel();

										kod.id = 4;
										kod.description = "NIT";

										try
										{
												String cnlDes = aData[(int)atrClient.canal];
												//Console.WriteLine("Canal: "+cnlDes);
												var cnl = db.Canal.Where(cn => cn.cnl_description.ToUpper() == cnlDes.ToUpper())
																	.Select(cn => new CanalViewModel { id = cn.cnl_id, description = cn.cnl_description })
																	.FirstOrDefault();



												if (cnl == null)
												{
														throw new Exception("* [No se encontró canal en la bd]: " + cnlDes);
												}

												String cityName = aData[(int)atrClient.city];
												//Console.WriteLine("Ciudad: " + cityName);
												var city = db.Cities.Where(ct => ct.cty_name.ToUpper() == cityName.ToUpper())
																						.Select(ct => new CityViewModel { id = ct.cty_id, departmentId = ct.dpt_id, name = ct.cty_name })
																						.FirstOrDefault();

												if (city == null)
												{
														throw new Exception("* [No se encontró la ciudad en la BD]: " + cityName);
												}

												String codeEconomicActivity = aData[(int)atrClient.economicActivity];

												if (codeEconomicActivity == "")
												{
														codeEconomicActivity = "0";
												}
												//Console.WriteLine("Actividad Economica: " + codeEconomicActivity);
												var economicActivity = db.EconomicActivity.Where(ea => ea.ea_code.ToString() == codeEconomicActivity)
																																	.Select(ea => new EconomicActivityViewModel { id = ea.ea_id, description = ea.ea_description })
																																	.FirstOrDefault();

												if (economicActivity == null)
												{
														throw new Exception("* [No se encontró la actividad economica en la BD]: " + codeEconomicActivity);
												}


												cli.kindOfDocument = kod;
												cli.id = aData[(int)atrClient.document];
												cli.name = aData[(int)atrClient.name];
												cli.canal = cnl;
												cli.city = city;
												cli.economicActivity = economicActivity;


												lsClient.Add(cli);
										}
										catch (Exception ex)
										{
												if (cont == 1)
												{
														continue;
												}
												Console.WriteLine(sline);
												Console.WriteLine("Registro # " + cont);
												Console.WriteLine(ex);
												continue;
										}



								}

								return lsClient;
						}

				}
				public void createClients(List<ClientViewModel> lsClients)
				{

						try
						{
								using (BDRAEntities db = new BDRAEntities())
								{
										foreach (var oClient in lsClients)
										{
												Client cli = new Client();
												cli.cli_document = oClient.id;
												cli.kod_id = oClient.kindOfDocument.id;
												cli.cli_name = oClient.name;
												cli.cnl_id = oClient.canal.id;
												cli.ea_id = oClient.economicActivity.id;
												cli.cty_id = oClient.city.id;
												cli.cli_registrationDate = DateTime.Now;
												db.Client.Add(cli);
												db.SaveChanges();
										}

								}
						}
						catch (Exception ex)
						{
								Console.WriteLine(ex);
								throw ex;
						}

				}


				public List<ContactViewModel> validateDataContact(List<string> lsData, string separator)
				{
						String sline = "--------------------------------------------------------------------------";
						var lsContacts = new List<ContactViewModel>();
						int cont = 0;
						ClientViewModel clientByContact = new ClientViewModel();
						JobTitleViewModel jobTitle = new JobTitleViewModel();
						BranchViewModel branchByContact = new BranchViewModel();

						string sDocumentByClient = "";
						string sNameByContact = "";
						string sJobTitle = "";
						string sPhone = "";
						string sCellPhone = "";
						string sAdress = "";
						string sEmail = "";

						try
						{
								using (BDRAEntities db = new BDRAEntities())
								{
										foreach (var data in lsData)
										{
												cont += 1;
												if (cont == 1)
												{
														continue;
												}

												Console.WriteLine(sline);
												Console.WriteLine("Validanto linea: " + cont);
												Console.WriteLine(data);
												var aData = data.Split(separator);

												sDocumentByClient = aData[(int)atrContact.clientDocument].ToString();

												clientByContact = db.Client.Where(cl => cl.cli_document == sDocumentByClient)
																										.Select(cl => new ClientViewModel { id = cl.cli_document, name = cl.cli_name })
																										.FirstOrDefault();

												if (clientByContact != null)
												{


														Console.WriteLine("Cliente: " + clientByContact.name);

														sNameByContact = aData[(int)atrContact.name].ToString();

														Console.WriteLine("Contacto: " + sNameByContact);

														var name = "";
														var lastName = "";

														var aName = sNameByContact.Split(" ");

														for (int i = 0; i < aName.Length; i++)
														{
																if (aName.Length == 2)
																{
																		if (i == 0)
																		{
																				name = aName[i];

																		}
																		else
																		{
																				lastName = aName[i];

																		}
																}
																else
																{
																		if (i <= 1)
																		{
																				name += aName[i] + " ";
																		}
																		else
																		{
																				lastName += aName[i] + " ";
																		}
																}
														}

														Console.WriteLine("Nombre del contacto: " + name);

														if (name.Trim() == "")
														{
																Console.WriteLine("Contacto no valido .............");
																continue;
														}
														Console.WriteLine("Apellido del contacto: " + lastName);

														sJobTitle = aData[(int)atrContact.jobTitle].ToString().ToUpper().Trim();
														sJobTitle = sJobTitle.Replace("  ", " ");


														Console.WriteLine("Cargo: " + sJobTitle);

														jobTitle = db.JobTitlesClient.Where(jt => jt.jtcl_description.ToUpper() == sJobTitle)
																												.Select(jt => new JobTitleViewModel { id = jt.jtcl_id, description = jt.jtcl_description })
																												.FirstOrDefault();


														if (jobTitle == null)
														{
																JobTitlesClient jt = new JobTitlesClient();
																jt.jtcl_description = sJobTitle;
																jt.jtcl_state = true;

																db.JobTitlesClient.Add(jt);
																db.SaveChanges();
																Console.WriteLine("Se crea cargo en la bd...");


																jobTitle = db.JobTitlesClient.Where(jt => jt.jtcl_description.ToUpper() == sJobTitle)
																														.Select(jt => new JobTitleViewModel { id = jt.jtcl_id, description = jt.jtcl_description })
																														.FirstOrDefault();
														}

														sPhone = aData[(int)atrContact.phone].ToString().Trim();
														Console.WriteLine("Teléfono: " + sPhone);
														sCellPhone = aData[(int)atrContact.cellphone].ToString().Trim();
														Console.WriteLine("Celular: " + sCellPhone);
														sAdress = aData[(int)atrContact.adress].ToString().Trim();
														Console.WriteLine("Dirección: " + sAdress);
														sEmail = aData[(int)atrContact.email].ToString().Trim();
														Console.WriteLine("Email: " + sEmail);

														branchByContact = db.branch.Where(b => b.cli_document == clientByContact.id && b.bra_isMain == true)
																														.Select(b => new BranchViewModel { id = b.bra_id, name = b.bra_name })
																														.FirstOrDefault();

														if (branchByContact == null)
														{
																branch branchCnt = new branch();
																branchCnt.bra_isMain = true;
																branchCnt.bra_state = true;
																branchCnt.bra_name = "PRINCIPAL " + clientByContact.name;
																branchCnt.cli_document = clientByContact.id;

																db.branch.Add(branchCnt);
																db.SaveChanges();

																branchByContact = db.branch.Where(b => b.cli_document == clientByContact.id && b.bra_isMain == true)
																														.Select(b => new BranchViewModel { id = b.bra_id, name = b.bra_name })
																														.FirstOrDefault();
														}


														Console.WriteLine("Sucursal id: " + branchByContact.id + " - " + branchByContact.name);

														ContactViewModel cnt = new ContactViewModel();
														cnt.branch = branchByContact;
														cnt.name = name;
														cnt.lastName = lastName;
														cnt.phone = sPhone;
														cnt.cellPhone = sCellPhone;
														cnt.adress = sAdress;
														cnt.email = sEmail;
														cnt.jobTitle = jobTitle;

														lsContacts.Add(cnt);

												}

										}
								}


						}
						catch (Exception ex)
						{
								Console.WriteLine(ex.Message);
						}




						return lsContacts;
				}

				public void createContacts(List<ContactViewModel> lsContacts, string separator, string headers, out List<string> lsDataError)
				{
						String sline = "--------------------------------------------------------------------------";
						int cont = 1;

						lsDataError = new List<string>();
						lsDataError.Add(headers);

						Console.WriteLine("Cantidad de Contactos a agregar: " + lsContacts.Count);

						using (BDRAEntities db = new BDRAEntities())
						{
								foreach (var oContact in lsContacts)
								{
										try
										{

												Console.WriteLine(sline);
												Console.WriteLine("Registro #: " + cont);
												Console.WriteLine("Branch: " + oContact.branch.id);
												Console.WriteLine("Nombres: " + oContact.name);
												Console.WriteLine("Apellidos: " + oContact.lastName);
												Console.WriteLine("Teléfono: " + oContact.phone);
												Console.WriteLine("Celular: " + oContact.cellPhone);
												Console.WriteLine("Dirección: " + oContact.adress);
												Console.WriteLine("Email: " + oContact.email);
												Console.WriteLine("Cargo: " + oContact.jobTitle.id);
												Console.WriteLine("Fecha de registro: " + DateTime.Now);

												Contact cnt = new Contact();
												cnt.bra_id = oContact.branch.id;
												cnt.cnt_name = oContact.name;
												cnt.cnt_lastName = oContact.lastName;
												cnt.cnt_phone = oContact.phone;
												cnt.cnt_cellPhone = oContact.cellPhone;
												cnt.cnt_adress = oContact.adress;
												cnt.cnt_email = oContact.email;
												cnt.jtcl_id = oContact.jobTitle.id;
												cnt.cnt_registrationDate = DateTime.Now;

												cnt.cty_id = null;
												cnt.cnt_costCenter = null;

												db.Contact.Add(cnt);
												db.SaveChanges();

												cont += 1;
										}
										catch (Exception ex)
										{
												var idClient = db.branch.Where(cl => cl.bra_id == oContact.branch.id).Select(cl => cl.cli_document).FirstOrDefault();
												var dataError = idClient + separator + oContact.name.Trim() + " " + oContact.lastName.Trim() + separator + oContact.jobTitle.description.Trim() + separator + oContact.phone.Trim() + separator + oContact.cellPhone.Trim() + separator + oContact.adress.Trim() + separator + oContact.email.Trim() + "\n";
												lsDataError.Add(dataError);
												Console.WriteLine("Error en registro: " + cont);
												Console.WriteLine(ex);
												cont += 1;
												db.Contact.Reverse();
												continue;
										}
								}
						}

				}


				public void showDataFromFile(List<String> lsData)
				{
						foreach (var data in lsData)
						{
								Console.WriteLine(lsData);
						}

				}


				public void createErrorFile(List<string> lsData, string type, ProgramConfig conf)
				{
						String sline = "--------------------------------------------------------------------------";
						Console.WriteLine(sline);
						Console.WriteLine("Se creara un archivo de error de tipo: " + type);
						var pathFileError = conf.pathError + "\\" + conf.nameFileError + "_" + type + "_" + DateTime.Now.ToString("yyyyMMdd") + ".csv";
						if (type == "contacts")
						{

								using (FileStream fileError = File.Open(pathFileError, FileMode.Create, FileAccess.ReadWrite))
								{
										foreach (string data in lsData)
										{
												Byte[] bData = new UTF8Encoding().GetBytes(data);
												fileError.Write(bData, 0, bData.Length);
										}
								}
						}
				}


				public List<RequestViewModel> validateDataRequests(List<String> lsData, string separator)
				{
						String sline = "--------------------------------------------------------------------------";
						var lsRequest = new List<RequestViewModel>();
						int cont = 1;
						try
						{
								using (BDRAEntities db = new BDRAEntities())
								{

										foreach (var data in lsData)
										{
												Console.WriteLine(sline);
												try
												{

														if (cont == 1)
														{
																cont += 1;
																continue;
														}
														Console.WriteLine("Registro #: " + cont);

														var aData = data.Split(separator);

														ClientViewModel oClient = new ClientViewModel();
														string sClientDocument = aData[(int)atrRequest.NIT].ToString().Trim();
														oClient = db.Client.Where(cl => cl.cli_document == sClientDocument)
																								.Select(cl => new ClientViewModel { id = cl.cli_document, name = cl.cli_name })
																								.FirstOrDefault();

														if (oClient == null)
														{
																throw new Exception("El cliente no existe en la base de datos.");
														}

														UserViewModel oManagerAccount = new UserViewModel();
														string sNameAndLastName = aData[(int)atrRequest.User].ToString().Trim();
														oManagerAccount = db.users.Where(us => (us.usu_name + " " + us.usu_lastName).Contains(sNameAndLastName))
																											.Select(us => new UserViewModel { id = us.usu_document })
																											.FirstOrDefault();

														if (oManagerAccount == null)
														{
																throw new Exception("El gerente de cuenta no se encuentra en la base de datos");
														}


														ContactViewModel oContactBank = new ContactViewModel();
														string sNameAndLastNameContactBank = aData[(int)atrRequest.ContactBank].ToString().Trim();

														oContactBank = db.Contact.Where(cnt => cnt.bra_id == 1 && (cnt.cnt_name + " " + cnt.cnt_lastName).Contains(sNameAndLastNameContactBank))
																										 .Select(cnt => new ContactViewModel { id = cnt.cnt_id, name = cnt.cnt_name, lastName = cnt.cnt_lastName })
																										 .FirstOrDefault();

														if (oContactBank == null)
														{
																Contact cntBnk = new Contact();
																cntBnk.bra_id = 1;

																var aNameLastName = divideNameAndLastName(sNameAndLastNameContactBank);
																cntBnk.cnt_name = aNameLastName[0];
																cntBnk.cnt_lastName = aNameLastName[1];
																cntBnk.jtcl_id = 1031;
																cntBnk.cnt_costCenter = "";
																cntBnk.cnt_registrationDate = DateTime.Now;

																db.Contact.Add(cntBnk);
																db.SaveChanges();


																oContactBank = db.Contact.Where(cnt => cnt.bra_id == 1 && (cnt.cnt_name + " " + cnt.cnt_lastName).Contains(sNameAndLastNameContactBank))
																										 .Select(cnt => new ContactViewModel { id = cnt.cnt_id, name = cnt.cnt_name, lastName = cnt.cnt_lastName })
																										 .FirstOrDefault();

														}


														DateTime dFisrtVisit = DateTime.Parse(aData[(int)atrRequest.FirstVisit].ToString().Trim());
														string sDateLastVisit = aData[(int)atrRequest.LastVisit].ToString().Trim();
														DateTime dLastVisit = new DateTime();
														if (sDateLastVisit != "")
														{
																dLastVisit = DateTime.Parse(sDateLastVisit);
														}

														string sFirstState = aData[(int)atrRequest.FirstState].ToString().Trim();
														StateViewModel oFirstState = new StateViewModel();

														oFirstState = db.states.Where(st => st.sta_description == sFirstState)
																									.Select(st => new StateViewModel { id = st.sta_id, description = st.sta_description })
																									.FirstOrDefault();

														if (oFirstState == null)
														{
																throw new Exception("No se encuentra el estado principal en la base de datos");
														}

														string sTirdState = aData[(int)atrRequest.TirdState].ToString().Trim();

														string sSecondState = "";
														if (sTirdState != "")
														{
																sSecondState = sTirdState;
														}
														else
														{
																sSecondState = aData[(int)atrRequest.SecondState].ToString().Trim();
														}

														StateViewModel oSecondState = new StateViewModel();
														oSecondState = db.states.Where(st => st.sta_description.Contains(sSecondState))
																									.Select(st => new StateViewModel { id = st.sta_id, description = st.sta_description })
																									.FirstOrDefault();

														if (oSecondState == null)
														{
																throw new Exception("No se encuentra el estado secundario en la base de datos");
														}


														string sProbability = aData[(int)atrRequest.Probability].ToString().Trim();
														ProbabilityViewModel oProbability = new ProbabilityViewModel();
														oProbability = db.probability.Where(pb => pb.prb_description.ToUpper() == sProbability.ToUpper())
																												.Select(pb => new ProbabilityViewModel { id = pb.prb_id, description = pb.prb_description })
																												.FirstOrDefault();

														if (oProbability == null)
														{
																throw new Exception("No se encuentra la probabilidad en la base de datos");
														}

														string sRiskState = aData[(int)atrRequest.RiskState].ToString().Trim();
														StateViewModel oRiskState = new StateViewModel();

														if (sRiskState != "")
														{
																oRiskState = db.states.Where(st => st.stGrp_id == 2 && st.sta_description.ToUpper() == sRiskState)
																												.Select(st => new StateViewModel { id = st.sta_id, description = st.sta_description })
																												.FirstOrDefault();
														}

														string sAmmountApproved = aData[(int)atrRequest.ApprovedAmount].ToString().Trim();

														string sRadicationDate = aData[(int)atrRequest.RiskRadication].ToString().Trim();

														RiskInformationViewModel rkInf = new RiskInformationViewModel();
														rkInf.riskState = oRiskState;
														if (sAmmountApproved == "")
														{
																sAmmountApproved = "0";
														}

														rkInf.ammountApproved = long.Parse(sAmmountApproved);
														rkInf.dateSubmissionAnalysis = null;
														string sUserUpdateRisk = aData[(int)atrRequest.UserUpdateRisk].ToString().Trim();											


														DateTime dRegistrationDate = DateTime.Parse(aData[(int)atrRequest.CreationDate].ToString());

														
														Console.WriteLine("Cliente: " + oClient.name);
														Console.WriteLine("Gerente de cuenta: " + oManagerAccount.id);
														Console.WriteLine("Gerente del banco: " + oContactBank.id);
														Console.WriteLine("Fecha primer visita: " + dFisrtVisit);
														Console.WriteLine("Fecha de la segunda visita: " + dLastVisit);
														Console.WriteLine("Estado principal: " + oFirstState.id);
														Console.WriteLine("Estado secundario: " + oSecondState.id);
														Console.WriteLine("Probabilidad: " + oProbability.id);
														Console.WriteLine("Fecha de registro: " + dRegistrationDate);
														Console.WriteLine("########################################");
														Console.WriteLine("Información de Riesgos");
														Console.WriteLine("########################################");
														Console.WriteLine("Estado de riesgo: " + rkInf.riskState.description);
														Console.WriteLine("Fecha de radicación riesgo: " + rkInf.dateSubmissionAnalysis);
														Console.WriteLine("Monto Aprobado: " + rkInf.ammountApproved);													
														Console.WriteLine("Usuario Actualización: ");														
														Console.WriteLine("########################################");
														Console.WriteLine("Información de Operaciones");
														Console.WriteLine("########################################");

														RequestViewModel rqt = new RequestViewModel();
														rqt.client = oClient;
														rqt.user = oManagerAccount;
														rqt.contact = oContactBank;
														rqt.initialDate = dFisrtVisit;
														rqt.lastDate = dLastVisit;
														rqt.parentState = oFirstState;
														rqt.childState = oSecondState;
														rqt.probability = oProbability;
														rqt.registrationDate = dRegistrationDate;
														rqt.riskInformation = new RiskInformationViewModel();
														rqt.riskInformation.riskState = oRiskState;

														lsRequest.Add(rqt);
														cont += 1;

												}
												catch (Exception ex)
												{														
														Console.WriteLine(ex);
														cont += 1;
														continue;
												}
										}

								}
						}
						catch (Exception ex)
						{
								Console.WriteLine("Problemas de ejecución en la base de datos");
								Console.WriteLine(ex.Message);
						}
						return lsRequest;
				}


				public void CreateRequests(List<RequestViewModel> lsRequests, string separator, string headers, out List<string> lsDataErrorRqt) {
						lsDataErrorRqt = new List<string>();
						int cont = 1;
						try
						{
								using (BDRAEntities db = new BDRAEntities()) {
										foreach (var rqt in lsRequests)
										{
												try {
														Console.WriteLine("Registro BD #: " + cont);
														Request oRqt = new Request();
														oRqt.rqt_registrationDate = rqt.registrationDate.Value;
														oRqt.rqt_firstVisitDate = rqt.initialDate.Value;
														oRqt.rqt_lastVisitDate = rqt.lastDate.Value;																										
														oRqt.prb_id = rqt.probability.id;
														oRqt.rqt_primaryState = rqt.parentState.id;
														oRqt.rqt_secondState = rqt.childState.id;
														oRqt.cli_document = rqt.client.id;
														oRqt.usu_document = rqt.user.id;
														oRqt.cnt_id = rqt.contact.id;
														oRqt.rqt_state = true;

														db.Request.Add(oRqt);
														db.SaveChanges();

														var lastRequest = db.Request.Where(r => r.cli_document == rqt.client.id)
																						 .OrderByDescending(r => r.rqt_registrationDate)
																						 .Select(r => new RequestViewModel { id = r.rqt_id })
																						 .FirstOrDefault();

														riskInformationByRequest riskInformation = new riskInformationByRequest();
														riskInformation.rqt_id = lastRequest.id;
														riskInformation.ribr_ammountApproved = 0;
														riskInformation.ribr_state = true;

														db.riskInformationByRequest.Add(riskInformation);
														db.SaveChanges();

														operationalInformationByRequest operationalInformation = new operationalInformationByRequest();
														operationalInformation.rqt_id = lastRequest.id;
														operationalInformation.oibr_deliveredAmmount = 0;
														operationalInformation.oibr_deliveredVehicles = 0;
														operationalInformation.oibr_state = true;

														db.operationalInformationByRequest.Add(operationalInformation);
														db.SaveChanges();


														cont++;
												} catch (Exception ex) { 
														Console.WriteLine(ex.Message);
														cont++;
														continue;
												}			
												

										}								
								}
						}
						catch (Exception ex)
						{
								Console.WriteLine(ex.Message);
						}
				}


				public string[] divideNameAndLastName(string data) {
						string[] aNameLastName = new string[2];
						var name = "";
						var lastName = "";

						var aName = data.Split(" ");

						for (int i = 0; i < aName.Length; i++)
						{
								if (aName.Length == 2)
								{
										if (i == 0)
										{
												name = aName[i];

										}
										else
										{
												lastName = aName[i];

										}
								}
								else
								{
										if (i <= 1)
										{
												name += aName[i] + " ";
										}
										else
										{
												lastName += aName[i] + " ";
										}
								}
						}

						aNameLastName[0] = name.Trim();
						aNameLastName[1] = lastName.Trim();

						return aNameLastName;
				}
		}
}
