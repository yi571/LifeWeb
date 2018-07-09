using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using LifeWeb.Models;

namespace LifeWeb.Controllers {
    public class LifeController : Controller {
        public IActionResult Index(string cName) {
            ViewBag.cName = cName;
            ViewBag.noData = "";
            if (string.IsNullOrEmpty(cName)) {
                //
                return View();
            } else {
                WebClient obj = new WebClient();
                obj.Encoding = Encoding.UTF8;  //設定編碼解決亂碼問題
                string url = string.Format("http://ngismap.forest.gov.tw/REST/species/ScientificName_C/{0}", cName);
                var str = obj.DownloadString(url);
                if (str == @"{""species_info"":[]}") {
                    ViewBag.noData = "查無「" + cName + "」相關資料...";
                } else {
                    JObject objJsonTxt = JObject.Parse(str);
                    JToken objNameTxtList = objJsonTxt.SelectToken("species_info");

                    List<CJData> jDatas = new List<CJData>();

                    foreach (JToken SpeciesInfo in objNameTxtList) {
                        var addData = new CJData {
                            name = SpeciesInfo.SelectToken("name").Value<string>(),
                            cName = SpeciesInfo.SelectToken("Common_name_c").Value<string>(),
                            nameCode = SpeciesInfo.SelectToken("name_code").Value<string>()
                        };
                        jDatas.Add(addData);

                    }
                    ViewBag.jDatas = jDatas;
                }
                
                //return Content(objNameTxtList.ToString());
            }

            return View();
            
        }

        public IActionResult NameCode(string nCode) {
            WebClient obj = new WebClient();
            obj.Encoding = Encoding.UTF8;  //設定編碼解決亂碼問題
            string urlDetail = string.Format("http://ngismap.forest.gov.tw/REST/species/name_code/{0}", nCode);
            string detailStr = obj.DownloadString(urlDetail);
            JObject objJsonTxt2 = JObject.Parse(detailStr);
            

            //中文科學分類
            JToken objWantDate = objJsonTxt2.SelectToken("kingdom_c");
            CTaxonomy taxonomyC = new CTaxonomy() {
                Kingdom = objWantDate.Value<string>(),
                Phylum = objJsonTxt2.SelectToken("phylum_c").Value<string>(),
                Cclass = objJsonTxt2.SelectToken("class_c").Value<string>(),
                Order = objJsonTxt2.SelectToken("order_c").Value<string>(),
                Family = objJsonTxt2.SelectToken("family_c").Value<string>(),
                Genus = objJsonTxt2.SelectToken("genus_c").Value<string>(),
                ScientificName = objJsonTxt2.SelectToken("ScientificName_c").Value<string>(),
                //Description = objJsonTxt2.SelectToken("species_eol_info[0].description").Value<string>(),
                //Habitat = objJsonTxt2.SelectToken("species_eol_info[0].habitat").Value<string>()
            };
            ViewBag.taxonomyC = taxonomyC;

            //英文科學分類
            JToken objWantDateE = objJsonTxt2.SelectToken("kingdom");
            CTaxonomy taxonomy = new CTaxonomy() {
                Kingdom = objWantDateE.Value<string>(),
                Phylum = objJsonTxt2.SelectToken("phylum").Value<string>(),
                Cclass = objJsonTxt2.SelectToken("class").Value<string>(),
                Order = objJsonTxt2.SelectToken("order").Value<string>(),
                Family = objJsonTxt2.SelectToken("family").Value<string>(),
                Genus = objJsonTxt2.SelectToken("genus").Value<string>(),
                ScientificName = objJsonTxt2.SelectToken("ScientificName").Value<string>(),
            };
            ViewBag.taxonomy = taxonomy;

            //簡介
            if (objJsonTxt2.SelectToken("species_eol_info[0].description") != null && objJsonTxt2.SelectToken("species_eol_info[0].habitat") != null) {
                CTaxonomy info = new CTaxonomy() {
                    Description = objJsonTxt2.SelectToken("species_eol_info[0].description").Value<string>(),
                    Habitat = objJsonTxt2.SelectToken("species_eol_info[0].habitat").Value<string>(),
                    Author = objJsonTxt2.SelectToken("species_eol_info[0].author").Value<string>(),
                    Distribution = objJsonTxt2.SelectToken("species_eol_info[0].distribution").Value<string>()
                };
                ViewBag.info = info;
            } else {
                CTaxonomy info = new CTaxonomy() {
                    Description = "無相關資料",
                    Habitat = "無相關資料"
                };
                ViewBag.info = info;
            }
            
            
            List<CImage> picUrls = new List<CImage>();
            //物種圖片
            JToken objWantPic = objJsonTxt2.SelectToken("img_info");
            foreach (JToken pictures in objWantPic) {
                var imgData = new CImage() {
                    ImgUrl = pictures.SelectToken("image_big").Value<string>(),
                    Author = pictures.SelectToken("author").Value<string>(),
                    License = pictures.SelectToken("license").Value<string>(),
                    Provider = pictures.SelectToken("provider").Value<string>()
            };
                
                picUrls.Add(imgData);
            }
            ViewBag.picUrls = picUrls;
            //return Content(nCode);
            return View();
        }
    }
}