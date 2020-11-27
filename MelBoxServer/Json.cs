using System.Web.Script.Serialization;

namespace MelBoxServer
{
    partial class Program
    {


        #region JSON


        public static string JSONSerialize(MelBoxGsm.Sms sms)
        {
            var js = new JavaScriptSerializer();
            return js.Serialize(sms);
        }

        public static string JSONSerialize(MelBoxGsm.GsmEventArgs telegram)
        {
            var js = new JavaScriptSerializer();
            return js.Serialize(telegram);
        }

        public static MelBoxGsm.Sms JSONDeserializeSms(string json)
        {
            var js = new JavaScriptSerializer();
            return js.Deserialize<MelBoxGsm.Sms>(json);
        }

        public static MelBoxGsm.GsmEventArgs JSONDeserializeTelegram(string json)
        {
            var js = new JavaScriptSerializer();
            return js.Deserialize<MelBoxGsm.GsmEventArgs>(json);
        }
        #endregion

    }
}
