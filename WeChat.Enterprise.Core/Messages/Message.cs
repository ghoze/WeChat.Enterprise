﻿using Flurl.Http;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace WeChat.Enterprise
{
    public abstract class Message
    {
        public WeChat WeChat { get; set; }

        public abstract string MessageType { get; }

        public bool Safe { get; set; }

        /// <summary>
        /// 将消息发送到指定的应用和目标。
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="targets"></param>
        /// <returns></returns>
        public Task<MessageSendResult> SendAsync(AgentKey agent, MessageSendTargets targets)
        {
            //https://qyapi.weixin.qq.com/cgi-bin/message/send?access_token=ACCESS_TOKEN
            return Task.Run(async () =>
            {
                var content = new JObject();
                FillSendContentBody(content, agent, targets);
                await GetMessageContentAsync(content, agent);
                var httpContent = new StringContent(content.ToString());
                var url = WeChat.GetAccessDomainUrl()
                  .AppendPathSegment("message")
                  .AppendPathSegment("send");
                var token = await WeChat.GetAccessTokenAsync(agent);
                ree:
                var result = await url.SetQueryParam("access_token", token.Token)
                .PostAsync(httpContent)
                .ReceiveJson();
                var errorCode = (int)result.errcode;
                if (WeChat.NeedRefreshAccessToken(errorCode))
                {
                    token = await WeChat.GetAccessTokenAsync(agent, true);
                    goto ree;
                }
                var errorMessage = (string)result.errmsg;
                if (errorCode != 0)
                {
                    throw new WeChatException(errorCode, errorMessage);
                }
                MessageSendTargets invalidTargets = new MessageSendTargets();
                if (targets.Users.Count > 0 || targets.ToAll)
                {
                    var users = (string)result.invaliduser;
                    if (!string.IsNullOrEmpty(users))
                    {
                        invalidTargets.Addusers(users.Split('|'));
                    }
                }
                if (!targets.ToAll && targets.Parties.Count > 0)
                {
                    var parties = (string)result.invalidparty;
                    if (!string.IsNullOrEmpty(parties))
                    {
                        invalidTargets.Addusers(parties.Split('|'));
                    }
                }

                if (!targets.ToAll && targets.Tags.Count > 0)
                {
                    var tags = (string)result.invalidtag;
                    if (!string.IsNullOrEmpty(tags))
                    {
                        invalidTargets.Addusers(tags.Split('|'));
                    }
                }
                return new MessageSendResult(errorCode, errorMessage, invalidTargets);
            });
        }

        protected abstract Task GetMessageContentAsync(JObject content, AgentKey agentKey);

        private void FillSendContentBody(JObject content, AgentKey agentKey, MessageSendTargets targets)
        {
            targets.AppendToObject(content);
            content.Add("agentid", agentKey.Id);
            content.Add("msgtype", MessageType);
            content.Add("safe", System.Convert.ToInt32(Safe));
        }
    }
}
