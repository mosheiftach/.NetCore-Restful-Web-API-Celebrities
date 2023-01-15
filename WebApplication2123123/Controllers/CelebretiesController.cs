using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.IO;
using WebApplication2123123.Models;

namespace WebApplication2123123.Controllers
{
    [EnableCors("*","*","*")]
    public class CelebretiesController : ApiController
    {
        public async Task<List<Celebrities>> Get()
        {
            List<Celebrities> DataList = new List<Celebrities>();
            string link = "https://www.imdb.com/list/ls052283250/";
            HttpClient hc = new HttpClient();
            HttpResponseMessage result = await hc.GetAsync(link);
            Stream stream = await result.Content.ReadAsStreamAsync();
            HtmlDocument doc = new HtmlDocument();
            doc.Load(stream);

            var HeaderNames = doc.DocumentNode.SelectNodes("//h3[@class='lister-item-header']//a");


            //Adding Scrapping to a List
            foreach (var item in HeaderNames)
            {
                var tempCeleb = new Celebrities();
                tempCeleb._key = item.OuterHtml.Substring(15, 24);
                tempCeleb._name = item.InnerText;
                DataList.Add(tempCeleb);
            }

            var RoleType = doc.DocumentNode.SelectNodes("//p[@class='text-muted text-small']");

            int j = 0;

            foreach (var item in RoleType)
            {
                int k = 0;
                string tempRole = item.InnerText.Substring(25) + " ";
                while (tempRole[k] != tempRole[tempRole.Length - 1])
                {
                    k++;
                }
                DataList[j]._role = tempRole.Substring(0, k);
                j++;
            }

            var img = doc.DocumentNode.SelectNodes("//div[@class='lister-item-image']//a//img");

            j = 0;

            foreach (var im in img)
            {
                string tempImage = im.Attributes["src"].Value;

                DataList[j]._image = tempImage;
                j++;
            }

            //Cleaning Data
            for (int i = 0; i < DataList.Count; i++)
            {
                DataList[i]._name = DataList[i]._name.Remove(DataList[i]._name.Length - 2, 2);
                DataList[i]._name = DataList[i]._name.Remove(0, 1);
            }

            foreach (var item in DataList)
            {
                item._key = item._key.Substring(0, 9);
                bool isFemale = false;
                string link2 = "https://www.imdb.com/name/" + item._key + "/?ref_=nmls_hd";
                result = await hc.GetAsync(link2);
                stream = await result.Content.ReadAsStreamAsync();
                doc = new HtmlDocument();
                doc.Load(stream);

                var GenderType = doc.DocumentNode.SelectNodes("//div[@class='infobar']");
                var DateOfBirth = doc.DocumentNode.SelectNodes("//div[@class='txt-block']//time");
                if (GenderType != null)
                {
                    foreach (var gender in GenderType)
                    {
                        string genderSelect = gender.InnerText;
                        if (genderSelect.Contains("Actress"))
                        {
                            isFemale = true;
                            break;
                        }
                    }
                }

                if (DateOfBirth != null)
                {
                    foreach (var date in DateOfBirth)
                    {
                        int left = 0;
                        int right = 0;
                        string genderSelect = date.InnerText.Remove(0, 1);
                        for (int y = 0; y < genderSelect.Length; y++)
                        {
                            if (genderSelect[y] == ',')
                            {
                                left = y + 1;
                            }

                            if ((genderSelect[y] == '1' || genderSelect[y] == '2')&& left != 0)
                            {
                                right = y;
                                break;
                            }
                        }

                        string genderSelect2 = genderSelect.Remove(left, right - left);
                        int spaceCouter = 0;
                        left = 0;
                        right = genderSelect2.Length - 1;

                        for (int w = 0; w < genderSelect2.Length; w++)
                        {
                            if (genderSelect2[w] == ' ')
                            {
                                spaceCouter++;
                            }

                            if (spaceCouter == 3)
                            {
                                left = w;
                                break;
                            }
                        }

                        string finalGenderSelect = genderSelect2.Remove(left, right - left);

                        item._dateOfBirth = finalGenderSelect;
                    }
                }

                if (isFemale)
                {
                    item._gender = "Female";
                }
                else
                {
                    item._gender = "Male";
                }


            }

            var fileName = "Celeb.json";
            JsonFileUtils.SimpleWrite(DataList, fileName);


            return DataList;

        }

        public List<Celebrities> Get(string i_key)
        {
            var dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Celeb.json");

            var data = System.IO.File.ReadAllText(dataPath);
            List<Celebrities> jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Celebrities>>(data);

            for (int i = 0; i < jsonObj.Count; i++)
            {
                if (jsonObj[i]._key.Contains(i_key))
                {
                    jsonObj.RemoveAt(i);
                }
            }

            string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
            if (System.IO.File.Exists(dataPath))
            {
                System.IO.File.Delete(dataPath);
            }
            System.IO.File.WriteAllText(dataPath, output);



            return jsonObj;
        }
    }
}

