using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using NisreSqlData;

namespace NisrePgData
{
    public class NisreDbException : Exception
    {
        
        public bool CustomException { get; set; }
        public Dictionary<string,string> ExceptionData = new Dictionary<string, string>();
        public override string Message { get;}
        
        
        public NisreDbException(SqlException exception)
        {
                ExceptionData.Add("procedure",exception.Procedure);
                ExceptionData.Add("server",exception.Server);
                ExceptionData.Add("source",exception.Source);
                ExceptionData.Add("message",exception.Message);

                this.CustomException = exception.Number > 50000;
                if (CustomException)
                {
                    this.Message = exception.Message;
                }
                else
                {
                    Message = "System Error";
                }
        }

        public NisreDbException(SqlException exception, SqlParam parameters)
        {
            ExceptionData.Add("procedure",exception.Procedure);
            ExceptionData.Add("parameters",parameters.GetParamatersInfo().ToJson());
            ExceptionData.Add("server",exception.Server);
            ExceptionData.Add("source",exception.Source);
            ExceptionData.Add("message",exception.Message);

            this.CustomException = exception.Number > 50000;
            if (CustomException)
            {
                this.Message = exception.Message;
            }
            else
            {
                Message = "System Error";
            }
        }

        public NisreDbException(Exception ex)
        {
            this.CustomException = false;
            this.Message = ex.Message;
        }
    }
}
