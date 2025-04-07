using System;
using System.Collections.Generic;
using GameCore.ECS.Core;

namespace GameCore.ECS.Events
{
    /// <summary>
    /// 事件系统，负责事件的发布和订阅
    /// </summary>
    public class EventSystem
    {
        // 事件处理器字典，按事件类型索引
        private readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();
        
        // 事件队列，存储待处理的事件
        private readonly Queue<EventWrapper> _eventQueue = new Queue<EventWrapper>();
        
        // 事件队列的锁，保证线程安全
        private readonly object _queueLock = new object();
        
        // 关联的世界实例
        private readonly World _world;

        /// <summary>
        /// 创建新的事件系统
        /// </summary>
        public EventSystem(World world)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
        }

        /// <summary>
        /// 发布事件
        /// </summary>
        public void Publish<T>(T eventData) where T : struct, IEvent
        {
            if (eventData.Equals(default(T)))
            {
                throw new ArgumentException("Event data cannot be default", nameof(eventData));
            }
            
            lock (_queueLock)
            {
                _eventQueue.Enqueue(new EventWrapper(typeof(T), eventData));
            }
        }

        /// <summary>
        /// 立即发布事件，不经过队列
        /// </summary>
        public void PublishImmediate<T>(T eventData) where T : struct, IEvent
        {
            ProcessEvent(typeof(T), eventData);
        }

        /// <summary>
        /// 订阅事件
        /// </summary>
        public void Subscribe<T>(Action<T> handler) where T : struct, IEvent
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            Type eventType = typeof(T);
            
            if (!_handlers.TryGetValue(eventType, out var handlers))
            {
                handlers = new List<Delegate>();
                _handlers[eventType] = handlers;
            }
            
            if (!handlers.Contains(handler))
            {
                handlers.Add(handler);
            }
        }

        /// <summary>
        /// 取消订阅事件
        /// </summary>
        public void Unsubscribe<T>(Action<T> handler) where T : struct, IEvent
        {
            if (handler == null)
            {
                return;
            }

            Type eventType = typeof(T);
            
            if (_handlers.TryGetValue(eventType, out var handlers))
            {
                handlers.Remove(handler);
            }
        }

        /// <summary>
        /// 处理队列中的所有事件
        /// </summary>
        public void ProcessEvents()
        {
            // 获取事件快照，避免在处理过程中修改队列
            List<EventWrapper> events = new List<EventWrapper>();
            
            lock (_queueLock)
            {
                while (_eventQueue.Count > 0)
                {
                    events.Add(_eventQueue.Dequeue());
                }
            }
            
            // 处理所有事件
            foreach (var eventWrapper in events)
            {
                ProcessEvent(eventWrapper.EventType, eventWrapper.EventData);
            }
        }

        /// <summary>
        /// 处理单个事件
        /// </summary>
        private void ProcessEvent(Type eventType, object eventData)
        {
            if (!_handlers.TryGetValue(eventType, out var handlers) || handlers.Count == 0)
            {
                return;
            }
            
            // 创建处理器的副本，避免在迭代过程中修改集合
            var handlersCopy = new List<Delegate>(handlers);
            
            foreach (var handler in handlersCopy)
            {
                try
                {
                    handler.DynamicInvoke(eventData);
                }
                catch (Exception ex)
                {
                    // 记录错误但不终止处理
                    System.Diagnostics.Debug.WriteLine($"Error handling event: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 事件包装器，用于队列存储
        /// </summary>
        private class EventWrapper
        {
            public Type EventType { get; }
            public object EventData { get; }

            public EventWrapper(Type eventType, object eventData)
            {
                EventType = eventType;
                EventData = eventData;
            }
        }
    }
} 