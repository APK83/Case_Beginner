using System.Net;

namespace NetChecker
{
    class SiteCheck
    {
        public static bool Test(string URL)
        {

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            request.Timeout = 3000;
            try
            {
                WebResponse resp = request.GetResponse();
            }
            catch
            {
                return false;
            }
            return true;

        }
    }
}
