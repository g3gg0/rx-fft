
using System.ServiceModel;
using Subsembly.SmartCard;
namespace SIMAuthDaemon
{
    [ServiceContract]
    public interface AuthService
    {
        [OperationContract]
        byte[] RunGsmAlgo(byte[] rand);
        [OperationContract]
        bool Available();
    }

    public class AuthServer : AuthService
    {
        #region ArmoryService Member

        public byte[] RunGsmAlgo(byte[] rand)
        {
            byte[] res = SIMAuthDaemonForm.GetInstance().RunGsmAlgo(rand);

            return res;
        }

        public bool Available()
        {
            return true;
        }

        #endregion
    }
}
