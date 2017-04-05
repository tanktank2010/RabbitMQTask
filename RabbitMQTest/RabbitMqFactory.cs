using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQTest
{
    public class RabbitMqFactory : IDisposable
    {
        private static RabbitMqFactory _simple;
        private static object _objS = new object();

        private IConnectionFactory _factory;
        private IConnection _connection;
        private object _obj;
        private Dictionary<string, IModel> _modelDict;

        public static RabbitMqFactory Get()
        {
            if (_simple == null)
            {
                lock (_objS)
                {
                    if (_simple == null)
                    {
                        _simple = new RabbitMqFactory();
                    }
                }
            }

            return _simple;
        }

        private RabbitMqFactory()
        {
            _obj = new object();
            _modelDict = new Dictionary<string, IModel>();
        }

        /// <summary>
        /// 初始化工厂
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public void Init(string hostname, string username, string password)
        {
            _factory = new ConnectionFactory() { HostName = hostname, UserName = username, Password = password };
        }

        /// <summary>
        /// 获取一个连接 如果不存在或者关闭 则重新开启一个
        /// </summary>
        /// <returns></returns>
        public IConnection GetConnection()
        {
            if (_factory == null)
            {
                throw new Exception("请先初始化");
            }

            if (_connection == null || !_connection.IsOpen)
            {
                lock (_obj)
                {
                    if (_connection != null)
                    {
                        _connection.Close();
                        _connection.Dispose();
                    }
                    _connection = _factory.CreateConnection();
                }
            }

            return _connection;
        }


        /// <summary>
        /// 获取(如果不存在新建)一个渠道
        /// </summary>
        /// <param name="channelName"></param>
        /// <param name="channeclwork"></param>
        /// <returns></returns>
        public IModel GetChannel(string channelName, Action<IModel> channeclwork)
        {
            IModel result = null;
            if (!_modelDict.ContainsKey(channelName) || _modelDict[channelName].IsOpen)
            {
                //因为如果连接失效并重连 因停止获取Channel的定义 所以用一个锁
                lock (_obj)
                {
                    if (_modelDict.ContainsKey(channelName))
                    {
                        _modelDict[channelName].Close();
                        _modelDict[channelName].Dispose();
                        _modelDict.Remove(channelName);
                    }
                    result = GetConnection().CreateModel();
                    channeclwork(result);
                    _modelDict.Add(channelName, result);
                }
            }

            return result;
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            try
            {
                DisposeModel();

                DisposeConnection();

                DisposeFactory();
            }
            catch (Exception ex)
            {

                Console.WriteLine("尝试关闭RMQ队列失败!" + ex.Message + " stack" + ex.StackTrace);
            }

            
        }

        /// <summary>
        /// 释放工厂
        /// </summary>
        private void DisposeFactory()
        {
            if (_factory != null)
            {
                _factory = null;
            }
        }

        /// <summary>
        /// 释放连接
        /// </summary>
        private void DisposeConnection()
        {
            if (_connection != null)
            {
                if (_connection.IsOpen)
                {
                    _connection.Close();
                }
                _connection.Dispose();
            }
        }

        /// <summary>
        /// 释放model
        /// </summary>
        private void DisposeModel()
        {
            if (_modelDict != null && _modelDict.Count > 0)
            {
                foreach (var item in _modelDict)
                {
                    if (item.Value != null)
                    {
                        if (item.Value.IsOpen)
                        {
                            item.Value.Close();
                        }
                        item.Value.Dispose();
                    }
                }
                _modelDict = null;
            }
        }
    }
}
