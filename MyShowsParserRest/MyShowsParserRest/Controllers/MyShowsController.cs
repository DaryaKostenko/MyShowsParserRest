using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MyShowsParserRest.Models;


namespace MyShowsParserRest.Controllers
{
    public class MyShowsController : ApiController
    {
        public IEnumerable<Show> GetAllShows()
        {
            return Parse.GetAllShows();
        }

        public Show GetShowById(string id)
        {
            var show = Parse.GetShowById(id);

            if (show == null)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent(string.Format("Не существует сериала с ID = {0}", id)),
                    ReasonPhrase = "Сериал не найден"
                };

                throw new HttpResponseException(resp);
            }

            return show;
        }

        [HttpPost]
        public void AddShowInDb([FromBody]Show show)
        {
            var result = Parse.AddShowInDB(show);
            if (result==false)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent(string.Format("Сериал с ID = {0} уже существует", show.Id)),
                    ReasonPhrase = "Введите другой Id"
                };

                throw new HttpResponseException(resp);
            }
        }

        [HttpDelete]
        public void DeleteArticle(string id)
        {
            Parse.DeleteShowFromDb(id);
        }

    }
}
