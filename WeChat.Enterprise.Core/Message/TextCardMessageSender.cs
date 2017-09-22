﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WeChat.Enterprise
{
    public sealed class TextCardMessageSender : MessageSender
    {
        private string _title;
        private string _gray, _heighlight, _normal;
        private string _url;
        private string _btnText;
        public TextCardMessageSender(WeChat weChat) : base(weChat)
        {
        }

        public TextCardMessageSender Title(string title)
        {
            _title = title;
            return this;
        }

        public TextCardMessageSender Url(string url)
        {
            _url = url;
            return this;
        }

        public TextCardMessageSender BtnText(string value)
        {
            _btnText = value;
            return this;
        }

        public TextCardMessageSender Gary(string value)
        {
            _gray = value;
            return this;
        }
        public TextCardMessageSender Normal(string value)
        {
            _normal = value;
            return this;
        }
        public TextCardMessageSender Highlight(string value)
        {
            _heighlight = value;
            return this;
        }

        protected override Task GetMessageContentAsync(JObject content, AgentKey agentKey)
        {
            return Task.Run(() =>
            {
                var cont = new JObject();
                cont.Add("title", _title);
                var sb = new StringBuilder();
                if (!string.IsNullOrEmpty(_gray))
                {
                    sb.AppendFormat("<div class=\"gray\">{0}</div>", _gray);
                }
                if (!string.IsNullOrEmpty(_normal))
                {
                    sb.AppendFormat("<div class=\"normal\">{0}</div>", _normal);
                }
                if (!string.IsNullOrEmpty(_heighlight))
                {
                    sb.AppendFormat("<div class=\"highlight\">{0}</div>", _heighlight);
                }
                cont.Add("description", sb.ToString());
                cont.Add("url", _url);
                content.Add("btntext", _btnText);
                content.Add("msgtype", "textcard");
                content.Add("textcard", cont);
            });
        }
    }
}
