using LibRedminePower.Extentions;
using LibRedminePower.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace LibRedminePower.Proxy
{
    /// <summary>
    /// https://www.c-sharpcorner.com/article/aspect-oriented-programming-in-c-sharp-using-dispatchproxy/
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LoggingAdvice<T> : DispatchProxy
    {
        private T _decorated;
        private Action _startAction;
        private Action _endAction;
        private Action<string> _logInfo;
        private Action<string> _logError;
        private Func<object, string> _serializeFunction;
        private Stopwatch sw;

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            if (targetMethod == null)
            {
                throw new ArgumentException(nameof(targetMethod));
            }

            try
            {
                _startAction?.Invoke();

                sw = new Stopwatch();
                sw.Start();

                var result = targetMethod.Invoke(_decorated, args);

                var resultTask = result as Task;
                if (resultTask != null)
                {
                    resultTask.ContinueWith(t =>
                    {
                        if (t.Exception != null)
                        {
                            logException(t.Exception.InnerException ?? t.Exception, targetMethod, args);
                        }
                        else
                        {
                            object taskResult = null;
                            if (t.GetType().GetTypeInfo().IsGenericType &&
                                t.GetType().GetGenericTypeDefinition() == typeof(Task<>))
                            {
                                var property = t.GetType().GetTypeInfo().GetProperties()
                                    .FirstOrDefault(p => p.Name == "Result");
                                if (property != null)
                                {
                                    taskResult = property.GetValue(t);
                                }
                            }

                            logCompleted(targetMethod, args);
                        }
                    }, TaskScheduler.Current);
                }
                else
                {
                    try
                    {
                        logCompleted(targetMethod, args);
                    }
                    catch (Exception ex)
                    {
                        //Do not stop method execution if an exception  
                        logException(ex, targetMethod, args);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                if (ex is TargetInvocationException)
                {
                    logException(ex.InnerException ?? ex, targetMethod, args);
                    throw ex.InnerException ?? ex;
                }
                else
                {
                    logException(ex, targetMethod, args);
                    throw;
                }
            }
            finally
            {
                _endAction?.Invoke();
            }
        }

        public static T Create(T decorated, Action startAction, Action endAction, Action<string> logInfo, Action<string> logError,
            Func<object, string> serializeFunction)
        {
            object proxy = Create<T, LoggingAdvice<T>>();
            ((LoggingAdvice<T>)proxy).setParameters(decorated, startAction, endAction, logInfo, logError, serializeFunction);

            return (T)proxy;
        }

        private void setParameters(T decorated, Action startAction, Action endAction, Action<string> logInfo, Action<string> logError,
            Func<object, string> serializeFunction)
        {
            if (decorated == null)
            {
                throw new ArgumentNullException(nameof(decorated));
            }

            _decorated = decorated;
            _startAction = startAction;
            _endAction = endAction;
            _logInfo = logInfo;
            _logError = logError;
            _serializeFunction = serializeFunction;
        }

        private void logException(Exception exception, MethodInfo methodInfo, object[] args)
        {
            try
            {
                var message = new StringBuilder();
                message.Append($"Failed to execute {methodInfo.Name} in {getExecTime()}.");
                message.Append($" ({createMethodInfoLog(methodInfo, args)}, ");
                message.Append($"Exception=[{exception.GetDescription()}] )");

                _logError?.Invoke(message.ToString());
            }
            catch (Exception)
            {
                // ignored  
                //Method should return original exception  
            }
        }

        private void logCompleted(MethodInfo methodInfo, object[] args)
        {
            var message = new StringBuilder();
            message.Append($"{methodInfo.Name} was completed in {getExecTime()}.");
            message.Append($" ({createMethodInfoLog(methodInfo, args)})");

            _logInfo?.Invoke(message.ToString());
        }

        private string getExecTime() => $"{string.Format("{0,5} [msec]", sw.ElapsedMilliseconds)}";

        private string createMethodInfoLog(MethodInfo methodInfo, object[] args)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append($"Parameters=[");
                if (args != null) sb.Append(string.Join(", ", methodInfo.GetParameters().Select((p, i) => $"{p.Name}:{getStringValue(args[i])}")));
                sb.Append($"], ");
                sb.Append($"ReturnParameter={getStringValue(methodInfo.ReturnParameter)}");
                return sb.ToString();
            }
            catch (Exception)
            {
                return "Error has occured in creating MethodInfo string";
            }
        }

        private string getStringValue(object obj)
        {
            if (obj == null) return "null";
            else if(obj is NameValueCollection collection)
            {
                var array = (from key in collection.AllKeys
                             from value in collection.GetValues(key)
                             select string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(value))).ToArray();
                return "?" + string.Join("&", array);
            }
            else if (obj is IEnumerable<object> enumerable)
            {
                return $"[{string.Join(", ", enumerable.Select(a => getStringValue(a)))}]";
            }

            if (obj.GetType().GetTypeInfo().IsPrimitive || obj.GetType().GetTypeInfo().IsEnum || obj is string)
                return obj.ToString();

            var dynMethod = obj.GetType().GetMethod("get_DebuggerDisplay", BindingFlags.NonPublic | BindingFlags.Instance);
            if(dynMethod != null)
            {
                var result = dynMethod.Invoke(obj, null) as string;
                return result.Replace(Environment.NewLine, "");
            }

            try
            {
                return _serializeFunction?.Invoke(obj) ?? obj.ToString();
            }
            catch
            {
                return obj.ToString();
            }
        }
    }
}
