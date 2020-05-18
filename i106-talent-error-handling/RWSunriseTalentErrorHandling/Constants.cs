using TalentErrorHandling.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace TalentErrorHandling
{
    internal class Constants
    {
        internal const string TableSchema = "dbo";
        internal const string WorkerErrorHandlingTable = "WORKER_ERROR_HANDLING";
        internal const string EmploymentStagingTable = "HcmEmploymentStaging";
        internal const string WorkerStagingTable = "HcmWorkerStaging";
        internal const string WorkerStagingHistoryTable = "HcmWorkerStaging_H";
        internal const string EmploymentDetailStagingHistoryTable = "HcmEmploymentDetailStaging_H";
        internal const string EmploymentEmployeeStagingHistoryTable = "HcmEmploymentEmployeeStaging_H";
        internal const string EmploymentTermStagingHistoryTable = "HcmEmploymentTermStaging_H";
        internal const string PositionDefaultDimensionStagingHistoryTable = "HcmPositionDefaultDimensionStaging_H";
        internal const string PositionHierarchyStagingHistoryTable = "HcmPositionHierarchyStaging_H";
        internal const string PositionV2StagingHistoryTable = "HcmPositionV2Staging_H";
        internal const string PositionWorkerAssignmentStagingHistoryTable = "HcmPositionWorkerAssignmentStaging_H";
        internal static readonly string LegalEntities = Environment.GetEnvironmentVariable("Legal_Entities", EnvironmentVariableTarget.Process);
        internal static readonly string LogicAppCDSWorkflowURL = Environment.GetEnvironmentVariable("CDS_CreateUpdate_TS", EnvironmentVariableTarget.Process);

        internal static string GetEnvironmentVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }

        internal List<MandatoryFields> WorkerMandatory()
        {
            //Mandatory Fields
            List<MandatoryFields> workerMandatoryFields = new List<MandatoryFields> {
                new MandatoryFields{FieldName="WorkerNumber",DisplayName="Personnel Number"},
                new MandatoryFields{FieldName="FirstName",DisplayName="First Name"},
                new MandatoryFields{FieldName="LastName",DisplayName="Last Name"},
                new MandatoryFields{FieldName="SeniorityDate",DisplayName="Seniority Date"},
                new MandatoryFields{FieldName="WorkerPositionFinancialDimension.JobTitle",DisplayName="Job Category"},
                new MandatoryFields{FieldName="WorkerPositionFinancialDimension.JobLevel",DisplayName="Job Level"},
                new MandatoryFields{FieldName="WorkerPositionFinancialDimension.FeeEarner",DisplayName="Fee Earner Type"},
                new MandatoryFields{FieldName="WorkerPositionFinancialDimension.ConsultantType",DisplayName="Consultant Type"},
                new MandatoryFields{FieldName="WorkerEmploymentTerm.AgreementTermId",DisplayName="Terms of Employment"},
                new MandatoryFields{FieldName="WorkerDateTimeCreatedUpdated.CreatedDateTime",DisplayName="CreatedDateTime"},
                new MandatoryFields{FieldName="WorkerPositionAssignment.PositionId",DisplayName="Position ID"},
                new MandatoryFields{FieldName="WorkerPositionAssignment.ValidFrom",DisplayName="Assignment start"},
                new MandatoryFields{FieldName="WorkerPosition.DepartmentNumber",DisplayName="Team"},
                new MandatoryFields{FieldName="WorkerPosition.LocationName",DisplayName="Location"},
                new MandatoryFields{FieldName="WorkerPosition.DepartmentName",DisplayName="Department"}

        };
            return workerMandatoryFields;
        }
        internal List<MandatoryFields> PostitionFDMandatory()
        {
            List<MandatoryFields> postionFDmandatoryFields = new List<MandatoryFields> {
                new MandatoryFields{FieldName="WorkerNumber",DisplayName="Personnel Number"},
                new MandatoryFields{FieldName="JobTitle",DisplayName="Job Category"},
                new MandatoryFields{FieldName="JobLevel",DisplayName="Job Level"},
                new MandatoryFields{FieldName="FeeEarner",DisplayName="Fee Earner Type"},
                new MandatoryFields{FieldName="ConsultantType",DisplayName="Consultant Type"}

        };
            return postionFDmandatoryFields;
        }
        internal List<MandatoryFields> EmploymentTermMandatory()
        {
            List<MandatoryFields> employmentTermmandatoryFields = new List<MandatoryFields> {
                new MandatoryFields{FieldName="WorkerNumber",DisplayName="Personnel Number"},
                new MandatoryFields{FieldName="AgreementTermId",DisplayName="Terms of Employment"}

        };
            return employmentTermmandatoryFields;
        }
        internal List<MandatoryFields> PositionAssignmentMandatory()
        {
            List<MandatoryFields> positionAssignmentmandatoryFields = new List<MandatoryFields> {
                new MandatoryFields{FieldName="WorkerNumber",DisplayName="Personnel Number"},
                new MandatoryFields{FieldName="PositionId",DisplayName="Position ID"},
                new MandatoryFields{FieldName="ValidFrom",DisplayName="Assignment start"}

        };
            return positionAssignmentmandatoryFields;
        }
        internal List<MandatoryFields> PositionMandatory()
        {
            List<MandatoryFields> positionmandatoryFields = new List<MandatoryFields> {
                new MandatoryFields{FieldName="WorkerNumber",DisplayName="Personnel Number"},
                new MandatoryFields{FieldName="DepartmentNumber",DisplayName="Team"},
                new MandatoryFields{FieldName="LocationName",DisplayName="Location"},
                new MandatoryFields{FieldName="DepartmentName",DisplayName="Department"}

        };
            return positionmandatoryFields;
        }
        internal List<LenghtRestrictions> WorkerLenghtFields()
        {
            //Length Restriction Fields
            List<LenghtRestrictions> lenghtRestrictions = new List<LenghtRestrictions> {
               new LenghtRestrictions{FieldName="ProfessionalSuffix",DisplayName="Professional Suffix",Length=50},
               new LenghtRestrictions{FieldName="ProfessinalTitle",DisplayName="Professinal Title",Length=50},
               new LenghtRestrictions{FieldName="EmailAddress",DisplayName="Email Address",Length=50},
               new LenghtRestrictions{FieldName="FirstName",DisplayName="First Name",Length=30},
               new LenghtRestrictions{FieldName="LastName",DisplayName="Last Name",Length=30},
               new LenghtRestrictions{FieldName="PreferredFirstName",DisplayName="Preferred First Name",Length=30},
               new LenghtRestrictions{FieldName="PreferredLastName",DisplayName="Preferred Last Name",Length=30},
               new LenghtRestrictions{FieldName="WorkerPosition.TitleId",DisplayName="Title ID",Length=50},
               new LenghtRestrictions{FieldName="WorkerPosition.DepartmentName",DisplayName="Department",Length=50}
            };
            return lenghtRestrictions;
        }
        internal List<LenghtRestrictions> PositionLenghtFields()
        {
            //Length Restriction Fields
            List<LenghtRestrictions> lenghtRestrictions = new List<LenghtRestrictions> {
               new LenghtRestrictions{FieldName="TitleId",DisplayName="Title ID",Length=50},
               new LenghtRestrictions{FieldName="DepartmentName",DisplayName="Department",Length=50}
            };
            return lenghtRestrictions;
        }
    }
}