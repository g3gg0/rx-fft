using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace GSM_Analyzer
{
    [ServiceContract]
    public interface AuthService
    {
        [OperationContract]
        byte[] RunGsmAlgo(byte[] rand);
        [OperationContract]
        bool Available();
    }
}
