﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using COURCEClientServer2.ObjectDataSender;
using Newtonsoft.Json;
using TextGUIModule;

namespace COURCEClientServer2.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        private List<string> _allCompileType = new List<string>();
        private ResultCompareObject _resultCompare = new ResultCompareObject();
        private DataBaseAPI _db = new DataBaseAPI();
        public ActionResult Index()
        {
            
            return View("MainPage");
        }

        private void GetResultList()
        {
            _resultCompare.MainCodeText = _db.GetOrignCodeFromId(_db.IdMainFileForHist);
            _resultCompare.ChildCodeText = _db.GetOrignCodeFromId(_db.IdiDenticalFie);
            _db.SetCodeMain(_db.IdMainFileForHist);
            _db.SetCodeChild(_db.IdiDenticalFie);
            double resVarnFish = _db.Code.ResultAlgorithm(1);
            double resVShiling = _db.Code.ResultAlgorithm(2);
            double resHeskel = _db.Code.ResultAlgorithm(0);
            _resultCompare.ResultCompare.Add(String.Format("Levenshtein Distance : {0:0.##}", resVarnFish));
            _resultCompare.ResultCompare.Add(String.Format("WShiling : {0:0.##}", resVShiling));
            _resultCompare.ResultCompare.Add(String.Format("Haskel : {0:0.##}", resHeskel));
            _db.AddingHistiry(resVarnFish, resVShiling, resHeskel);
        }
        [HttpPost]
        public async Task<string> GetComipeType(string lang)
        {
            string result = await Task.Run(() =>
            {
                _allCompileType = _db.GetCompile(lang);
                string s = JsonConvert.SerializeObject(_allCompileType);
                return s;
            });
            return result;
        }

        [HttpPost]
        public async Task<string> AddCode(AddingCodeObject param)
        {
            bool isOver = await _db.AddingSubmit(param.Name, param.Description, param.CompileType, param.Code, param.IsSearch, param.FileMane);
            if (param.IsSearch)
            {
                GetResultList();
            }

            if (param.CompareLocal)
            {
                _resultCompare.TokkingMainCode = _db.GetMainCodeList();
                _resultCompare.TokkingChildCode =_db.GetChildCodeList();
            }
            return JsonConvert.SerializeObject(_resultCompare);
        }

        [HttpGet]
        public string GetListSubmit()
        {
            List<string> listAllSubmit = _db.DescSubm();
            string result = JsonConvert.SerializeObject(listAllSubmit);
            return result;
        }

        [HttpPost]
        public string SearchFromListSubmit(string tagForSearch)
        {
            _db.SearchIn(tagForSearch);
            GetResultList();
            return JsonConvert.SerializeObject(_resultCompare);
        }

        [HttpGet]
        public string GetListHistory()
        {
            List<string> listAllHistory = _db.GetListHistory();
            string result = JsonConvert.SerializeObject(listAllHistory);
            return result;
        }

        [HttpPost]
        public string Registration(RegistrationObject regInfo)
        {
            _db.RegistsAccount(regInfo.Name, regInfo.EMail, regInfo.Password);
            return JsonConvert.SerializeObject(true);
        }

        [HttpPost]
        public string Autification(string login, string password)
        {
            List<object> resultAfterAutif = new List<object>()
            {
                _db.Autification(login, password),
                _db.GetNameFromLogin(login),
                JsonConvert.SerializeObject(_db.GetImageUser(login))
            };
            return JsonConvert.SerializeObject(resultAfterAutif);
        }

        [HttpPost]
        public string ChangeUserImage(string sendImage, string name)
        {
            byte[] data = JsonConvert.DeserializeObject<byte[]>(sendImage);
            _db.UpdateImage(name, data);
            return JsonConvert.SerializeObject(true);
        }

        [HttpPost]
        public void UpdateUserInfo(string email, string name, string password)
        {
            if (name != null)
            {
                _db.ChangeName(name, email);
            }
            if (password != null)
            {
                _db.ChangePassword(password, email);
            }
        }
    }
}