//using EZGO.CMS.LIB.Interfaces;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Net.Http.Headers;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Threading.Tasks;
//using WebApp.Logic.Interfaces;

//namespace WebApp.Controllers
//{
//    public class DownloadController
//    {
//        [HttpPost]
//        [Route("/download/image")]
//        public async Task<IActionResult> Image([FromBody] string url)
//        {
//            try
//            {
//                using (var client = new WebClient())
//                using (var stream = client.OpenRead(url))
//                {
//                    var imageData = client.DownloadData(new Uri(url));

//                    return new FileStreamResult(new MemoryStream(imageData), new MediaTypeHeaderValue(client.ResponseHeaders[HttpResponseHeader.ContentType]))
//                    {
//                        FileDownloadName = Path.GetFileName(url),
//                    };
//                }
//            }
//            catch (Exception ex)
//            {
//                return new BadRequestResult();
//            }
//        }
//    }
//}
