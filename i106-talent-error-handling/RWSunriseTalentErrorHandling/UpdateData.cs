using System;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace TalentErrorHandling
{
    internal class UpdateData
    {

        public async Task<string> UpdateSQLTable(object worker, string mandatoryFieldsCheck, string lengthRestrictionFieldsCheck, string workerType)
        {
            string result = string.Empty;
            string workerNumber = worker.GetPropValue("WorkerNumber").Item2;
            string positionId = string.Empty;
            string legalEntityID = string.Empty;
            string firstName = string.Empty;
            string lastName = string.Empty;
            string workerCreatedTime = string.Empty;
            int workerCreatedHrs = 0;
            DateTime workerCreatedOn = DateTime.Now;

            if (workerType == "Workers")
            {
                legalEntityID = worker.GetPropValue("WorkerEmploymentDetail.LegalEntityId").Item2;
                positionId = worker.GetPropValue("WorkerEmploymentDetail.PositionId").Item2;
                firstName = worker.GetPropValue("FirstName").Item2;
                lastName = worker.GetPropValue("LastName").Item2;
                workerCreatedTime = worker.GetPropValue("WorkerDateTimeCreatedUpdated.CreatedDateTime").Item2;
                workerCreatedOn = DateTime.Parse(workerCreatedTime);
            }
            else
            {
                legalEntityID = worker.GetPropValue("LegalEntityId").Item2;
                positionId = worker.GetPropValue("PositionId").Item2;
            }

            //check if Worker is present in worker_error_handling table
            using (SqlConnection conn = new SqlConnection(SQLConn.ConnStr()))
            {
                StringBuilder sb = new StringBuilder();

                sb.Append($"select * from [{Constants.TableSchema}].[{Constants.WorkerErrorHandlingTable}] " +
                    $"where WORKERNUMBER = @workernumber and POSITIONID = @positionId");

                using (SqlCommand command = new SqlCommand(sb.ToString(), conn))
                {
                    command.Parameters.AddWithValue("@workernumber", workerNumber);
                    command.Parameters.AddWithValue("@positionId", positionId);

                    await conn.OpenAsync();

                    using (SqlDataReader dr = await command.ExecuteReaderAsync())
                    {
                        //Get LegalEntityID
                        if (legalEntityID == string.Empty)
                        {
                            sb.Clear();
                            sb.Append($"select LEGALENTITYID from [{Constants.TableSchema}].[{Constants.EmploymentStagingTable}] " +
                                $" Where PERSONNELNUMBER = @workerNumber" +
                                $" and LEGALENTITYID in ({Constants.LegalEntities})");


                            using (SqlCommand cmd = new SqlCommand(sb.ToString(), conn))
                            {
                                cmd.Parameters.AddWithValue("@workerNumber", workerNumber);
                                using (SqlDataReader datarow = await cmd.ExecuteReaderAsync())
                                {
                                    if (datarow.HasRows)
                                    {
                                        while (await datarow.ReadAsync())
                                        {
                                            legalEntityID = datarow["LEGALENTITYID"].ToString();
                                        }
                                    }
                                }
                            }

                        }

                        //if worker present, update data
                        if (dr.HasRows)
                        {
                            //if mandatoryField or lengthRestrictions are not empty, add worker to worker_error_handling table
                            if (!string.IsNullOrEmpty(mandatoryFieldsCheck) || !string.IsNullOrEmpty(lengthRestrictionFieldsCheck))
                            {
                                sb.Clear();
                                sb.Append($"UPDATE [{Constants.TableSchema}].[{Constants.WorkerErrorHandlingTable}]" +
                                    $" SET MANDATORYFIELDSCHECK = @mandatoryFieldsCheck," +
                                    $" LENGTHRESTRICTIONSCHECK = @lengthRestrictionFieldsCheck," +
                                    $" LEGALENTITYID = @legalEntityId," +
                                    $" MAIL_SENT = @mail_sent" +
                                    $" where WORKERNUMBER = @workerNumber and" +
                                    $" POSITIONID = @positionId");

                                using (SqlCommand commandUpdate = new SqlCommand(sb.ToString(), conn))
                                {
                                    commandUpdate.Parameters.AddWithValue("@workerNumber", workerNumber);
                                    commandUpdate.Parameters.AddWithValue("@positionId", positionId);
                                    commandUpdate.Parameters.AddWithValue("@legalEntityId", legalEntityID);
                                    commandUpdate.Parameters.AddWithValue("@mandatoryFieldsCheck", mandatoryFieldsCheck);
                                    commandUpdate.Parameters.AddWithValue("@lengthRestrictionFieldsCheck", lengthRestrictionFieldsCheck);
                                    commandUpdate.Parameters.AddWithValue("@mail_sent", 'N');
                                    await commandUpdate.ExecuteNonQueryAsync();
                                }

                                result = $"Worker:{workerNumber}, PositionId:{positionId} :Invalid Data.";
                            }
                            else
                            {
                                sb.Clear();
                                sb.Append($"DELETE from [{Constants.TableSchema}].[{Constants.WorkerErrorHandlingTable}]" +
                                   $" where WORKERNUMBER = @workerNumber and POSITIONID = @positionId");

                                using (SqlCommand commandUpdate = new SqlCommand(sb.ToString(), conn))
                                {
                                    commandUpdate.Parameters.AddWithValue("@workerNumber", workerNumber);
                                    commandUpdate.Parameters.AddWithValue("@positionId", positionId);
                                    await commandUpdate.ExecuteNonQueryAsync();
                                }
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(mandatoryFieldsCheck) || !string.IsNullOrEmpty(lengthRestrictionFieldsCheck))
                            {
                                //Get FirstName,LastName
                                if (firstName == string.Empty || lastName == string.Empty)
                                {
                                    sb.Clear();
                                    sb.Append($"select FIRSTNAME,LASTNAME from [{Constants.TableSchema}].[{Constants.WorkerStagingTable}] " +
                                        $" Where PERSONNELNUMBER = @workerNumber");


                                    using (SqlCommand cmd = new SqlCommand(sb.ToString(), conn))
                                    {
                                        cmd.Parameters.AddWithValue("@workerNumber", workerNumber);
                                        using (SqlDataReader datarow = await cmd.ExecuteReaderAsync())
                                        {
                                            if (datarow.HasRows)
                                            {
                                                while (await datarow.ReadAsync())
                                                {
                                                    firstName = datarow["FIRSTNAME"].ToString();
                                                    lastName = datarow["LASTNAME"].ToString();
                                                }
                                            }
                                        }
                                    }

                                }
                                //Check if Worker was created morethan 3hours before
                                if (string.IsNullOrEmpty(workerCreatedTime))
                                {
                                    GetDateTimeInCDS dt = new GetDateTimeInCDS();
                                    workerCreatedOn = await dt.GetWorkerCreatedUpdate(workerNumber);
                                }
                                workerCreatedHrs = Convert.ToInt32(DateTime.Now.Subtract(workerCreatedOn).TotalHours);
                                if (workerCreatedHrs > 3)
                                {
                                    //create worker
                                    sb.Clear();
                                    sb.Append($"insert into [{Constants.TableSchema}].[{Constants.WorkerErrorHandlingTable}] " +
                                        $"([WORKERNUMBER],[POSITIONID],[WORKERNAME],[LEGALENTITYID],[MANDATORYFIELDSCHECK],[LENGTHRESTRICTIONSCHECK],[MAIL_SENT])" +
                                        $" values(@workerNumber,@positionId,@workerName,@legalEntityId, @mandatoryFieldsCheck, @lengthRestrictionFieldsCheck, @mail_sent )");

                                    using (SqlCommand commandUpd = new SqlCommand(sb.ToString(), conn))
                                    {
                                        commandUpd.Parameters.AddWithValue("@workerNumber", workerNumber);
                                        commandUpd.Parameters.AddWithValue("@positionId", positionId);
                                        commandUpd.Parameters.AddWithValue("@workerName", firstName + " " + lastName);
                                        commandUpd.Parameters.AddWithValue("@legalEntityId", legalEntityID);
                                        commandUpd.Parameters.AddWithValue("@mandatoryFieldsCheck", mandatoryFieldsCheck);
                                        commandUpd.Parameters.AddWithValue("@lengthRestrictionFieldsCheck", lengthRestrictionFieldsCheck);
                                        commandUpd.Parameters.AddWithValue("@mail_sent", 'N');

                                        await commandUpd.ExecuteNonQueryAsync();
                                    }
                                }
                                result = $"Worker:{workerNumber}, PositionId:{positionId} :Invalid Data.";
                            }
                        }
                    }
                    //reset To_Be_Processed flag in few tables, so that sync tool wont pick this user in next run
                    if (!string.IsNullOrEmpty(mandatoryFieldsCheck) || !string.IsNullOrEmpty(lengthRestrictionFieldsCheck))
                    {
                        await ResetToBeProcessedFlag(worker, workerNumber, positionId, workerType, workerCreatedHrs);
                    }
                    else
                    {
                        if (workerType != "Workers")
                        {
                            //Check if WorkerStaging flag is 'I', if so, Reset it to 'Y', so that Worker will be processed 
                            sb.Clear();
                            sb.Append($"update [{Constants.TableSchema}].[{Constants.WorkerStagingHistoryTable}] " +
                                $" set TO_BE_PROCESSED = 'Y' " +
                                $" Where PERSONNELNUMBER = @workerNumber" +
                                $" and TO_BE_PROCESSED = 'I'");

                            using (SqlCommand commandUpdate = new SqlCommand(sb.ToString(), conn))
                            {
                                commandUpdate.Parameters.AddWithValue("@workerNumber", workerNumber);
                                await commandUpdate.ExecuteNonQueryAsync();
                            }
                        }
                    }
                }
                return result;
            }
        }

        public async Task ResetToBeProcessedFlag(object worker, string workerNumber, string positionID, string workerType, int workerCreatedHrs)
        {

            string ExecutionID = worker.GetPropValue("ExecutionIdWorker").Item2;
            char workerStagingFlag = 'N';
            //Get ExecutionIdWorker
            string workerExecutionID, EmploymentDetailExecutionID, EmploymentEmployeeExecutionID, EmploymentTermExecutionID, PositionDefaultDimensionExecutionID, PositionHierarchyExecutionID, PositionV2ExecutionID, PositionWorkerAssignmentExecutionID = null;
            switch (workerType)
            {
                case "Workers":
                    workerExecutionID = worker.GetPropValue("ExecutionIdWorker").Item2;
                    EmploymentDetailExecutionID = worker.GetPropValue("WorkerEmploymentDetail.ExecutionIdEmpDetail").Item2;
                    EmploymentEmployeeExecutionID = worker.GetPropValue("EmployeeDetail.ExecutionId").Item2;
                    EmploymentTermExecutionID = worker.GetPropValue("WorkerEmploymentTerm.ExecutionEmpTerm").Item2;
                    PositionDefaultDimensionExecutionID = worker.GetPropValue("WorkerPositionFinancialDimension.ExecutionIdPositionFD").Item2;
                    PositionHierarchyExecutionID = worker.GetPropValue("WorkerPositionHierarchy.ExecutionIdPositionHierarchy").Item2;
                    PositionV2ExecutionID = worker.GetPropValue("WorkerPosition.ExecutionIdPosition").Item2;
                    PositionWorkerAssignmentExecutionID = worker.GetPropValue("WorkerPositionAssignment.ExecutionIdPositionAssign").Item2;
                    if (workerCreatedHrs <= 3)
                    {
                        workerStagingFlag = 'I';
                    }
                    break;
                case "Employments":
                    workerExecutionID = string.Empty;
                    EmploymentDetailExecutionID = string.Empty;
                    EmploymentEmployeeExecutionID = string.Empty;
                    EmploymentTermExecutionID = string.Empty;
                    PositionDefaultDimensionExecutionID = string.Empty;
                    PositionHierarchyExecutionID = string.Empty;
                    PositionV2ExecutionID = string.Empty;
                    PositionWorkerAssignmentExecutionID = string.Empty;
                    break;
                case "Positions":
                    workerExecutionID = string.Empty;
                    EmploymentDetailExecutionID = string.Empty;
                    EmploymentEmployeeExecutionID = string.Empty;
                    EmploymentTermExecutionID = string.Empty;
                    PositionDefaultDimensionExecutionID = string.Empty;
                    PositionHierarchyExecutionID = string.Empty;
                    PositionV2ExecutionID = worker.GetPropValue("ExecutionIdPosition").Item2;
                    PositionWorkerAssignmentExecutionID = string.Empty;
                    break;
                case "PositionFinancialDimensions":
                    workerExecutionID = string.Empty;
                    EmploymentDetailExecutionID = string.Empty;
                    EmploymentEmployeeExecutionID = string.Empty;
                    EmploymentTermExecutionID = string.Empty;
                    PositionDefaultDimensionExecutionID = worker.GetPropValue("ExecutionIdPositionFD").Item2;
                    PositionHierarchyExecutionID = string.Empty;
                    PositionV2ExecutionID = string.Empty;
                    PositionWorkerAssignmentExecutionID = string.Empty;
                    break;
                case "EmploymentDetails":
                    workerExecutionID = string.Empty;
                    EmploymentDetailExecutionID = worker.GetPropValue("ExecutionIdEmpDetail").Item2;
                    EmploymentEmployeeExecutionID = string.Empty;
                    EmploymentTermExecutionID = string.Empty;
                    PositionDefaultDimensionExecutionID = string.Empty;
                    PositionHierarchyExecutionID = string.Empty;
                    PositionV2ExecutionID = string.Empty;
                    PositionWorkerAssignmentExecutionID = string.Empty;
                    break;
                case "EmploymentTerms":
                    workerExecutionID = string.Empty;
                    EmploymentDetailExecutionID = string.Empty;
                    EmploymentEmployeeExecutionID = string.Empty;
                    EmploymentTermExecutionID = worker.GetPropValue("ExecutionEmpTerm").Item2;
                    PositionDefaultDimensionExecutionID = string.Empty;
                    PositionHierarchyExecutionID = string.Empty;
                    PositionV2ExecutionID = string.Empty;
                    PositionWorkerAssignmentExecutionID = string.Empty;
                    break;
                case "PositionHierarchy":
                    workerExecutionID = string.Empty;
                    EmploymentDetailExecutionID = string.Empty;
                    EmploymentEmployeeExecutionID = string.Empty;
                    EmploymentTermExecutionID = string.Empty;
                    PositionDefaultDimensionExecutionID = string.Empty;
                    PositionHierarchyExecutionID = worker.GetPropValue("ExecutionIdPositionHierarchy").Item2;
                    PositionV2ExecutionID = string.Empty;
                    PositionWorkerAssignmentExecutionID = string.Empty;
                    break;
                case "EmployeeDetails":
                    workerExecutionID = string.Empty;
                    EmploymentDetailExecutionID = string.Empty;
                    EmploymentEmployeeExecutionID = worker.GetPropValue("ExecutionID").Item2;
                    EmploymentTermExecutionID = string.Empty;
                    PositionDefaultDimensionExecutionID = string.Empty;
                    PositionHierarchyExecutionID = string.Empty;
                    PositionV2ExecutionID = string.Empty;
                    PositionWorkerAssignmentExecutionID = string.Empty;
                    break;
                case "PositionAssignments":
                    workerExecutionID = string.Empty;
                    EmploymentDetailExecutionID = string.Empty;
                    EmploymentEmployeeExecutionID = string.Empty;
                    EmploymentTermExecutionID = string.Empty;
                    PositionDefaultDimensionExecutionID = string.Empty;
                    PositionHierarchyExecutionID = string.Empty;
                    PositionV2ExecutionID = string.Empty;
                    PositionWorkerAssignmentExecutionID = worker.GetPropValue("ExecutionIdPostionAssign").Item2;
                    break;
                default:
                    workerExecutionID = string.Empty;
                    EmploymentDetailExecutionID = string.Empty;
                    EmploymentEmployeeExecutionID = string.Empty;
                    EmploymentTermExecutionID = string.Empty;
                    PositionDefaultDimensionExecutionID = string.Empty;
                    PositionHierarchyExecutionID = string.Empty;
                    PositionV2ExecutionID = string.Empty;
                    PositionWorkerAssignmentExecutionID = string.Empty;
                    break;
            }


            using (SqlConnection conn = new SqlConnection(SQLConn.ConnStr()))
            {
                StringBuilder sb = new StringBuilder();

                //Reseting for WorkerStagingHistoryTable
                sb.Append($"update [{Constants.TableSchema}].[{Constants.WorkerStagingHistoryTable}] " +
                   $" set TO_BE_PROCESSED = @workerStagingFlag " +
                   $" Where PERSONNELNUMBER = @workerNumber" +
                   $" and EXECUTIONID = @executionId");

                using (SqlCommand command = new SqlCommand(sb.ToString(), conn))
                {
                    command.Parameters.AddWithValue("@workerStagingFlag", workerStagingFlag);
                    command.Parameters.AddWithValue("@workerNumber", workerNumber);
                    command.Parameters.AddWithValue("@executionId", workerExecutionID);

                    await conn.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }

                //Reseting for EmploymentDetailStagingHistoryTable

                sb.Clear();
                sb.Append($"update [{Constants.TableSchema}].[{Constants.EmploymentDetailStagingHistoryTable}] " +
                    $" set TO_BE_PROCESSED = 'N' " +
                    $" Where PERSONNELNUMBER = @workerNumber" +
                    $" and EXECUTIONID = @executionId");
                using (SqlCommand command = new SqlCommand(sb.ToString(), conn))
                {
                    command.Parameters.AddWithValue("@workerNumber", workerNumber);
                    command.Parameters.AddWithValue("@executionId", EmploymentDetailExecutionID);
                    await command.ExecuteNonQueryAsync();
                }

                //Reseting for EmploymentEmployeeStagingHistoryTable


                sb.Clear();
                sb.Append($"update [{Constants.TableSchema}].[{Constants.EmploymentEmployeeStagingHistoryTable}] " +
                    $" set TO_BE_PROCESSED = 'N' " +
                    $" Where PERSONNELNUMBER = @workerNumber" +
                    $" and EXECUTIONID = @executionId");
                using (SqlCommand command = new SqlCommand(sb.ToString(), conn))
                {
                    command.Parameters.AddWithValue("@workerNumber", workerNumber);
                    command.Parameters.AddWithValue("@executionId", EmploymentEmployeeExecutionID);
                    await command.ExecuteNonQueryAsync();
                }


                //Reseting for EmploymentTermStagingHistoryTable
                sb.Clear();
                sb.Append($"update [{Constants.TableSchema}].[{Constants.EmploymentTermStagingHistoryTable}] " +
                    $" set TO_BE_PROCESSED = 'N' " +
                    $" Where PERSONNELNUMBER = @workerNumber" +
                    $" and EXECUTIONID = @executionId");
                using (SqlCommand command = new SqlCommand(sb.ToString(), conn))
                {
                    command.Parameters.AddWithValue("@workerNumber", workerNumber);
                    command.Parameters.AddWithValue("@executionId", EmploymentTermExecutionID);
                    await command.ExecuteNonQueryAsync();
                }

                //Reseting for PositionDefaultDimensionStagingHistoryTable
                sb.Clear();
                sb.Append($"update [{Constants.TableSchema}].[{Constants.PositionDefaultDimensionStagingHistoryTable}] " +
                    $" set TO_BE_PROCESSED = 'N' " +
                    $" Where POSITIONID = @positionId" +
                    $" and EXECUTIONID = @executionId");
                using (SqlCommand command = new SqlCommand(sb.ToString(), conn))
                {
                    command.Parameters.AddWithValue("@positionId", positionID);
                    command.Parameters.AddWithValue("@executionId", PositionDefaultDimensionExecutionID);
                    await command.ExecuteNonQueryAsync();
                }



                //Reseting for PositionHierarchyStagingHistoryTable
                sb.Clear();
                sb.Append($"update [{Constants.TableSchema}].[{Constants.PositionHierarchyStagingHistoryTable}] " +
                    $" set TO_BE_PROCESSED = 'N' " +
                    $" Where POSITIONID = @positionId" +
                    $" and EXECUTIONID = @executionId");
                using (SqlCommand command = new SqlCommand(sb.ToString(), conn))
                {
                    command.Parameters.AddWithValue("@positionId", positionID);
                    command.Parameters.AddWithValue("@executionId", PositionHierarchyExecutionID);
                    await command.ExecuteNonQueryAsync();
                }

                //Reseting for PositionV2StagingHistoryTable
                sb.Clear();
                sb.Append($"update [{Constants.TableSchema}].[{Constants.PositionV2StagingHistoryTable}] " +
                    $" set TO_BE_PROCESSED = 'N' " +
                    $" Where POSITIONID = @positionId" +
                    $" and EXECUTIONID = @executionId");
                using (SqlCommand command = new SqlCommand(sb.ToString(), conn))
                {
                    command.Parameters.AddWithValue("@positionId", positionID);
                    command.Parameters.AddWithValue("@executionId", PositionV2ExecutionID);
                    await command.ExecuteNonQueryAsync();
                }

                //Reseting for PositionWorkerAssignmentStagingHistoryTable
                sb.Clear();
                sb.Append($"update [{Constants.TableSchema}].[{Constants.PositionWorkerAssignmentStagingHistoryTable}] " +
                    $" set TO_BE_PROCESSED = 'N' " +
                    $" Where POSITIONID = @positionId" +
                    $" and EXECUTIONID = @executionId");
                using (SqlCommand command = new SqlCommand(sb.ToString(), conn))
                {
                    command.Parameters.AddWithValue("@positionId", positionID);
                    command.Parameters.AddWithValue("@executionId", PositionWorkerAssignmentExecutionID);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}