using LibRedminePower.Exceptions;
using Redmine.Net.Api;
using Redmine.Net.Api.Exceptions;
using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibRedminePower.Properties;
using System.Net;

namespace LibRedminePower.Extentions
{
    public static class RedmineManagerExtenstions
    {
        public static User GetCurrentUserWithErrConv(this IRedmineManager manager, NameValueCollection parameters = null)
        {
            return getObject(() => manager.GetCurrentUser(parameters), nameof(GetCurrentUserWithErrConv));
        }

        public static T GetObjectWithErrConv<T>(this IRedmineManager manager, string id, NameValueCollection parameters = null) where T : class, new()
        {
            return getObject(() => manager.GetObject<T>(id, parameters != null ? parameters : new NameValueCollection()), HttpVerbs.GET, id);
        }

        public static List<T> GetObjectsWithErrConv<T>(this IRedmineManager manager, params string[] include) where T : class, new()
        {
            return getObjects(() => manager.GetObjects<T>(include), HttpVerbs.GET);
        }

        public static List<T> GetObjectsWithErrConv<T>(this IRedmineManager manager, NameValueCollection parameters) where T : class, new()
        {
            return getObjects(() => manager.GetObjects<T>(parameters), HttpVerbs.GET);
        }

        public static List<T> GetObjectsWithErrConv<T>(this IRedmineManager manager, NameValueCollection parameters, List<string> additionalQueries) where T : class, new()
        {
            return getObjects(() => manager.GetObjects<T>(parameters, additionalQueries), HttpVerbs.GET);
        }

        public static List<T> GetObjectsWithErrConv<T>(this IRedmineManager manager, int projectId) where T : class, new()
        {
            return getObjects(() => manager.GetObjects<T>(new NameValueCollection { { RedmineKeys.PROJECT_ID, projectId.ToString() } }), HttpVerbs.GET);
        }

        public static List<T> GetObjectsWithErrConv<T>(this IRedmineManager manager, string redmineKey, List<string> values, NameValueCollection param = null) where T : class, new()
        {
            if (param != null)
                param.Add(redmineKey, string.Join(",", values));
            else
                param = new NameValueCollection() { { redmineKey, string.Join(",", values) } };
            return getObjects(() => manager.GetObjects<T>(param), HttpVerbs.GET);
        }

        public static T CreateObjectWithErrConv<T>(this IRedmineManager manager, T obj) where T : class, new()
        {
            return getObject(() => manager.CreateObject(obj, null), HttpVerbs.POST);
        }

        public static void UpdateObjectWithErrConv<T>(this IRedmineManager manager, string id, T obj) where T : class, new()
        {
            exec<T>(() => manager.UpdateObject(id, obj, null), HttpVerbs.PUT, id);
        }

        public static void DeleteObjectWithErrConv<T>(this IRedmineManager manager, string id, NameValueCollection parameters = null) where T : class, new()
        {
            exec<T>(() => manager.DeleteObject<T>(id, parameters), HttpVerbs.DELETE, id);
        }

        public static List<WikiPage> GetAllWikiPagesWithErrConv(this IRedmineManager manager, string projIdentifier)
        {
            return getObjects(() => manager.GetAllWikiPages(projIdentifier), HttpVerbs.GET);
        }

        private static void exec<T>(Action action, string method, string id)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception e)
            {
                convertException(e, method, typeof(T), id);
                throw;
            }
        }

        private static T getObject<T>(Func<T> action, string method, string id = null)
        {
            try
            {
                return action.Invoke();
            }
            catch (Exception e)
            {
                convertException(e, method, typeof(T), id);
                throw;
            }
        }

        private static List<T> getObjects<T>(Func<List<T>> action, string method)
        {
            try
            {
                var result = action.Invoke();
                return result != null ? result : new List<T>();
            }
            catch (Exception e)
            {
                convertException(e, method, typeof(T));
                throw;
            }
        }

        private static void convertException(Exception e, string httpVerb, Type type, string id = null)
        {
            var re = e.InnerException as RedmineException;
            if (re != null)
            {
                throw new RedmineApiException(httpVerb, type, re, id);
            }
        }
    }

    public static class RedmineExceptionExtensions
    {
        public static string ToErrMsg(this RedmineException e)
        {
            switch (e)
            {
                case RedmineTimeoutException t:
                    return Resources.ApiErrMsgTimeout;
                case NameResolutionFailureException nrf:
                    return Resources.ApiErrMsgNameResolutionFailure;
                case NotFoundException nf:
                    return Resources.ApiErrMsgNotFound;
                case UnauthorizedException u:
                    return Resources.ApiErrMsgUnauthorized;
                case ForbiddenException f:
                    return Resources.ApiErrMsgForbidden;
                case ConflictException c:
                    return Resources.ApiErrMsgConflict;
                case NotAcceptableException na:
                    // Redmine.Net.Api.Extensions.WebExceptionExtensions.HandleWebException にて、
                    // response.StatusDescription を Message に設定している。
                    // よって na.Message を返すようにする。
                    return na.Message;
                default:
                    return Resources.ApiErrMsgUnexpected;
            }
        }
    }
}
