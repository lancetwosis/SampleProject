using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using LibRedminePower.Enums;
using LibRedminePower.Exceptions;
using LibRedminePower.Extentions;
using LibRedminePower.Logging;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace LibRedminePower.Models.Manager
{
    public class RedmineWebManager : Bases.ModelBase
    {
        private string urlBase;
        private string userName;
        private string password;
        private HttpClient client;

        private Regex regRefsIssuePattern = new Regex(@"(refs|references|IssueID) +#\d+", RegexOptions.Compiled);
        private const string cUrlIssuePrefix = "/issues/";
        private const string cUrlChange = "#change-";

        public RedmineWebManager(string urlBase, string userName, string password)
        {
            this.urlBase = urlBase;
            this.userName = userName;
            this.password = password;
            client = new HttpClient(new HttpClientHandler()
            {
                UseCookies = true,
                CookieContainer = new System.Net.CookieContainer(),
                UseDefaultCredentials = true,
            });
        }

        public RedmineWebManager(string urlBase, string userName, string password, string userNameForBasicAuth, string passwordForBasicAuth)
            : this(urlBase, userName, password)
        {
            var prm = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", userNameForBasicAuth, passwordForBasicAuth)));
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", prm);
        }

        public async Task<List<ActivityModel>> GetActivitiesAsync(CancellationToken token, int userId, DateTime start, DateTime end)
        {
            using (var log = Logger.CreateProcess<RedmineWebManager>($"GetActivities userId={userId}, start={start}, end={end}"))
            {
                try
                {
                    await loginAsync(token);

                    DateRange range = null;
                    var result = new List<ActivityModel>();
                    var days = start.GetDays(end).Where(a => DateTime.Today >= a).ToList();

                    foreach (var day in days.OrderByDescending(a => a))
                    {
                        if (range == null || !range.Includes(day))
                        {
                            var doc = await getWebActivityAsync(token, userId, day);
                            var subTitle = doc.QuerySelectorWithNullCheck(".subtitle");

                            var isJp = getLocation(doc);
                            range = updateRange(range, subTitle.TextContent, isJp);

                            var children = doc.QuerySelectorWithNullCheck("#activity").Children;
                            if (children == null) continue;

                            var dayActivities = children.Chunk(2).ToList();
                            foreach (var dayActivity in dayActivities)
                            {
                                var webDateStr = dayActivity.First().TextContent;
                                var webDate = webDateStr == (isJp ? "今日" : "Today") ? DateTime.Today : DateTimeExtentions.Parse(webDateStr);
                                var activities = getActivities(token, dayActivity.Last(), webDate);
                                result.AddRange(activities);
                            }
                        }
                    }

                    return result.Where(a => start <= a.Date && a.Date <= end).ToList();
                }
                catch (Exception e)
                {
                    throw new ApplicationContinueException($"Failed to GetActivitiesAsync. userId={userId}, start={start}, end={end}", e);
                }
            }
        }

        private async Task loginAsync(CancellationToken token)
        {
            using (var log = Logger.CreateProcess<RedmineWebManager>("loginAsync"))
            using (var response = await client.GetAsync(new Uri(new Uri(urlBase), "login").AbsoluteUri, token))
            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var reader = new StreamReader(stream, Encoding.GetEncoding("UTF-8")))
            {
                var html = await reader.ReadToEndAsync().WithCancel(token);
                var parser = new HtmlParser();
                var doc = await parser.ParseDocumentAsync(html);
                var input_userName = doc.QuerySelector("input[name='username']") as IHtmlInputElement;
                if (input_userName != null)
                {
                    var authenticity_token = doc.QuerySelector("input[name='authenticity_token']") as IHtmlInputElement;

                    //ログイン用のPOSTデータ生成
                    var content = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        { "authenticity_token", authenticity_token.Value },
                        { "back_url", "/" },
                        { "username", userName },
                        { "password", password },
                    });

                    var formDiv = doc.GetElementById("login-form") as IHtmlDivElement;
                    var formElement = formDiv.QuerySelector("form") as IHtmlFormElement;
                    var actionUrl = formElement.Action;
                    var loginUrl = new Uri(new Uri(urlBase), actionUrl).AbsoluteUri;

                    using (var response2 = await client.PostAsync(loginUrl, content))
                    using (var stream2 = await response2.Content.ReadAsStreamAsync())
                    using (var reader2 = new StreamReader(stream2, Encoding.GetEncoding("UTF-8")))
                    {
                        // ログイン情報を POST するとログインの成功後、リダイレクトが実施される
                        // しかし現状リダイレクトが実施されず 401 が返される現象がある
                        // そのためステータスコードでの確認は行わない

                        var html2 = await reader2.ReadToEndAsync().WithCancel(token);
                        var parser2 = new HtmlParser();
                        var doc2 = await parser2.ParseDocumentAsync(html2).WithCancel(token);

                        // ログインに失敗すると、id=flash_error が含まれるhtmlが返ってくるので、エラーとする。
                        var isFailed = doc2.GetElementById("flash_error") != null;
                        if(isFailed)
                            throw new RedmineLoginFailedException();
                    }
                }
                else
                {
                    // ログインが成功している状態でログイン画面にアクセスするとリダイレクトが実施される
                    // しかし上記と同じ（と思われる）現象で 401 が返される。よってここでは無視する。
                }
            }
        }

        public bool getLocation(IHtmlDocument activityPage)
        {
            // doc.Title は以下のように日本語の場合 "活動" から始まる。よってそこで Redmine の言語設定を判断する。
            // 英語：　"Activity - User Name - Redmine"
            // 日本語："活動 - User Name - Redmine"
            if (activityPage.Title.StartsWith("活動"))
                return true;
            else if (activityPage.Title.StartsWith("Activity"))
                return false;
            else
                throw new ApplicationException($"Failed to getLocation. activityPage.Children={activityPage.GetChildrenSummary()}");
        }

        private DateRange updateRange(DateRange pre, string subTitle, bool isJp)
        {
            var spliters = isJp ? new[] { "から", "まで" } : new[] { "From", "to" };
            var fromTo = subTitle.Split(spliters, StringSplitOptions.RemoveEmptyEntries);
            var from = DateTimeExtentions.Parse(fromTo.First());
            if (pre == null)
                return new DateRange(from, DateTimeExtentions.Parse(fromTo.Last()));
            else
                return new DateRange(from, pre.End);
        }

        private async Task<IHtmlDocument> getWebActivityAsync(CancellationToken token, int userId, DateTime targetDate)
        {
            var activityUrl = urlBase + $"activity?&from={targetDate.ToString("yyyy-MM-dd")}&user_id={userId}";
            using (var log = Logger.CreateProcess<RedmineWebManager>($"getWebActivityAsync Url={activityUrl}"))
            {
                // 活動ページの内容を取得する。
                var doc = await getHtmlDocAsync(token, activityUrl);
                if (doc.StatusCode != HttpStatusCode.OK)
                    throw new RedmineConnectionException(Properties.Resources.errGetWebActivity, doc.StatusCode, activityUrl);
                else if (doc.Document == null)
                    throw new ApplicationException("Failed to getHtmlDoc. doc.Document=null");

                return doc.Document;
            }
        }

        private async Task<(HttpStatusCode StatusCode, IHtmlDocument Document)> getHtmlDocAsync(CancellationToken token, string url)
        {
            try
            {
                using (var response = await client.GetAsync(url, token))
                using (var stream = await response.Content.ReadAsStreamAsync().WithCancel(token))
                using (var reader = new StreamReader(stream, Encoding.GetEncoding("UTF-8")))
                {
                    var html = await reader.ReadToEndAsync().WithCancel(token);
                    var parser = new HtmlParser();
                    return (response.StatusCode, await parser.ParseDocumentAsync(html).WithCancel(token));
                }
            }
            catch (Exception e)
            {
                throw new RedmineConnectionException(Properties.Resources.errReadWebPage, url, e);
            }
        }

        private List<ActivityModel> getActivities(CancellationToken token, IElement dl, DateTime webDate)
        {
            if (dl.Children == null)
                return new List<ActivityModel>();

            return dl.Children.Chunk(2).Select(pair =>
            {
                token.ThrowIfCancellationRequested();

                var itemDt = pair.First();
                if (itemDt.ClassName.Contains("time-entry")) return null;
                var itemDd = pair.Last();
                try
                {
                    var message = string.Join(Environment.NewLine, itemDd.Children.Where(a => a.ClassName != "author").Select(a => a.TextContent));
                    var time = DateTimeExtentions.Parse(itemDt.Children.Single(a => a.ClassName == "time").TextContent);
                    var myEnd = time.SetDay(webDate);
                    var project = itemDt.Children.Single(a => a.ClassName == "project").TextContent;
                    var content = itemDt.Children.Single(a => a.LocalName == "a").TextContent;
                    var ticketUrl = itemDt.Children.Single(a => a.LocalName == "a").GetAttribute("href");
                    string ticketNo = null;
                    string changeNo = null;
                    string url = null;
                    if (ticketUrl.Contains(cUrlIssuePrefix))
                    {
                        var startIndex = ticketUrl.IndexOf(cUrlIssuePrefix) + cUrlIssuePrefix.Length;
                        var endIndex = ticketUrl.IndexOf(cUrlChange);
                        ticketNo = (endIndex == -1) ?
                            ticketUrl.Substring(startIndex) :
                            ticketUrl.Substring(startIndex, endIndex - startIndex);
                        changeNo = endIndex > 0 ? ticketUrl.Substring(endIndex + cUrlChange.Length) : null;
                        url = new Uri(new Uri(urlBase), ticketUrl).AbsoluteUri;
                    }
                    else if (content.Contains("#"))
                    {
                        ticketNo = regRefsIssuePattern.Matches(content).Cast<Match>().FirstOrDefault()?.Value;
                        if (!string.IsNullOrEmpty(ticketNo))
                            ticketNo = ticketNo.Substring(ticketNo.IndexOf('#') + 1);
                    }
                    return new ActivityModel() { ProjectName = project, Subject = content, Description = message, Date = myEnd, IssueId = ticketNo, ChangeNo = changeNo, Url = url };
                }
                catch (Exception e)
                {
                    var msg = $"Failed to create ActivityModel. itemDt=[{itemDt?.TextContent}], itemDd=[{itemDd?.TextContent}], dl.Children={dl.GetChildrenSummary()}";
                    throw new ApplicationException(msg, e);
                }
            }).Where(a => a != null).ToList();
        }

        public async Task<MarkupLangType> GetMarkupLangTypeFromTicketAsync(string ticketUrl)
        {
            var token = new CancellationTokenSource().Token;
            await loginAsync(token);
            var doc = await getHtmlDocAsync(token, ticketUrl);

            if (doc.StatusCode != HttpStatusCode.OK)
                throw new RedmineConnectionException(Properties.Resources.errReadWebPageWithStatusCode, doc.StatusCode, ticketUrl);

            // チケットのソースには以下のような script が含まれるのでそこで判定する。
            // <script src="/javascripts/jstoolbar/markdown.js?1627804851">
            var scripts = doc.Document.QuerySelectorAll("script");
            if (scripts.Any(s => s.OuterHtml.Contains("textile.js")))
                return MarkupLangType.Textile;
            else if (scripts.Any(s => s.OuterHtml.Contains("markdown.js")))
                return MarkupLangType.Markdown;
            else
                return MarkupLangType.None;
        }

        public async Task<List<(string Tracker, int Id, string Subject)>> GetParentIssuesAsync(string ticketUrl)
        {
            var token = new CancellationTokenSource().Token;
            await loginAsync(token);
            var doc = await getHtmlDocAsync(token, ticketUrl);

            if (doc.StatusCode != HttpStatusCode.OK)
                throw new RedmineConnectionException(Properties.Resources.errReadWebPageWithStatusCode, doc.StatusCode, ticketUrl);

            var subject = doc.Document.QuerySelector("div.subject");

            var results = new List<(string Tracker, int Id, string Subject)>();
            foreach (var p in subject.QuerySelectorAll("p"))
            {
                // 「作業 #403: 優先度「急いで」のチケット」のような文字列が設定されている
                var m = Regex.Match(p.TextContent, "^(.+) #([0-9]+): (.+)$");
                if (m.Success)
                {
                    results.Add((m.Groups[1].ToString(), int.Parse(m.Groups[2].ToString()), m.Groups[3].ToString()));
                }
            }
            return results;
        }

        class DateRange
        {
            public DateRange(DateTime start, DateTime end)
            {
                Start = start;
                End = end;
            }

            public DateTime Start { get; private set; }
            public DateTime End { get; private set; }

            public bool Includes(DateTime value)
            {
                return (Start <= value) && (value <= End);
            }
        }
    }
}
