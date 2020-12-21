using i5.Toolkit.Core.Utilities;
using i5.Toolkit.Core.Utilities.ContentLoaders;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Threading.Tasks;

namespace i5.Toolkit.MixedReality.Utilities.ContentLoaders
{
    /// <summary>
    /// Content loader which fetches string data using MRTK's Rest.GetAsync
    /// </summary>
    public class MRTKRestLoader : IContentLoader<string>
    {
        /// <summary>
        /// Loads content using MRTK's Web access classes
        /// </summary>
        /// <param name="uri">The uri which should be loaded</param>
        /// <returns>Returns a WebResponse with the content of the web request</returns>
        public async Task<WebResponse<string>> LoadAsync(string uri)
        {
            Response resp = await Rest.GetAsync(uri);
            return FromResponse(resp);
        }

        // converts an MRTK Response to a WebResponse
        private WebResponse<string> FromResponse(Response resp)
        {
            WebResponse<string> webResponse = new WebResponse<string>(resp.Successful, resp.ResponseBody, resp.ResponseData, resp.ResponseCode, resp.ResponseBody);
            return webResponse;
        }
    }
}