using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using LiteDB;

namespace MyShowsParserRest.Models
{
    public class Parse
    {
        public const string NameDb = @"D:\Учеба\лэти\1 семестр\Инструментальные средства программирования(C#)\Projects\MyShowsParserConsole\MyShowsWcfService\MyShowsWcfService\binShowsDB.db";
        public const string NameCollection = "showsID";

        //поиск информации по ключу
        public static Show GetShowInfo(string id)
        {
            Show show = new Show();
            string htmlShowId = "https://myshows.me/view/" + id + "/";
            HtmlWeb web = new HtmlWeb();
            HtmlDocument htmlDoc = web.Load(htmlShowId);

            try
            {
                show.Id = id;
                //Название сериала
                show.Name = htmlDoc.DocumentNode.SelectSingleNode("//main/h1[@itemprop='name']").InnerText.Trim(); ;
                //Оригинальное название
                show.OriginalName = htmlDoc.DocumentNode.SelectSingleNode("//main/p[@class='subHeader']").InnerText.Trim();
                //show.Image = htmlDoc.DocumentNode.SelectSingleNode(".//div[@class = 'presentBlock']").InnerHtml.Trim().Substring(34).Remove(79);
                //информация из таблицы
                var info = htmlDoc.DocumentNode.SelectNodes(".//div[@class = 'clear']/p");
                foreach (var str in info)
                {
                    if (str.InnerText.Contains("Страна"))
                        show.Country = str.InnerText.Trim().Substring(8);

                    else if (str.InnerText.Contains("Жанры"))
                        show.Genres = str.InnerText.Replace(" ", string.Empty).Replace("\n", " ").Substring(7);

                    else if (str.InnerText.Contains("Рейтинг MyShows"))
                        show.MyShowsRating =
                            str.InnerText.Trim().Replace("\n", " ").Replace("&thinsp;", string.Empty).Substring(17);
                }
                AddShowInDB(show);
                return show;
            }
            catch (Exception)
            {
                return null;
            }

        }

        //возвращает ид сериала при поиске по слову 
        public static string GetShowId(string htmlShowId)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument htmlDoc = web.Load(htmlShowId);
            try
            {
                //ссылка на первый найденный сериал
                string link =
                    htmlDoc.DocumentNode.SelectSingleNode("//main/table[@class='catalogTable']/tr/td/a").Attributes[0]
                        .Value.Substring(24);// выделить ид
                return link.Remove(link.Length - 1);//удалить символ / в конце
            }
            catch (Exception)
            {
                return String.Empty;
            }
        }

        //добавление в кэш
        public static bool AddShowInDB(Show show)
        {
            // открывает базу данных, если ее нет - то создает
            using (var db = new LiteDatabase(NameDb))
            {
                // Получаем коллекцию
                var collectionShows = db.GetCollection<Show>(NameCollection);
                //проверка для внесения в базу сериала, параметры которого задаются на стороне клиента
                var resultSearch = collectionShows.FindOne(x => x.Id.Equals(show.Id));
                if (resultSearch==null)
                {
                    //добавляем новый элемент
                    collectionShows.Insert(show);
                    return true;
                }
                return false;
            }
        }

        //удалить из кэша
        public static void DeleteShowFromDb(string id)
        {
            // открывает базу данных, если ее нет - то создает
            using (var db = new LiteDatabase(NameDb))
            {
                // Получаем коллекцию
                var collectionShows = db.GetCollection<Show>(NameCollection);
                //добавляем новый элемент
                collectionShows.Delete(x=>x.Id.Equals(id));
            }
        }

        //поиск в кэше
        public static Show SearchInDB_ID(string ID)
        {
            // открывает базу данных, если ее нет - то создает
            using (var db = new LiteDatabase(NameDb))
            {
                // Получаем коллекцию
                var collectionShows = db.GetCollection<Show>(NameCollection);
                var resultSearch = collectionShows.FindOne(x => x.Id.Equals(ID));
                return resultSearch;
            }
        }

        //поиск фильма по Id
        public static Show GetShowById(string id)
        {
            try
            {
                var searchRes = SearchInDB_ID(id);
                if (searchRes == null) //если в кэше нет
                    return GetShowInfo(id);
                else
                {
                    return searchRes;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        
        public static IEnumerable<Show> GetAllShows()
        {
            using (var db = new LiteDatabase(NameDb))
            {
                // Получаем коллекцию
                var collectionShows = db.GetCollection<Show>(NameCollection);
                var resultSearch = collectionShows.FindAll();
                return resultSearch;
            }
        }

    }
}