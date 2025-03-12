using System;
using System.Collections.Generic;
using Kharazmi.Common.Events;
using Kharazmi.Common.Json;
using Kharazmi.Common.ValueObjects;
using Newtonsoft.Json;

namespace Kharazmi.Demo1.Data
{
    public static class WizardApp
    {
        public enum EditorType
        {
            None,
            DOCUPLOAD,
            NUMBERINPUT,
            DATEINPUT,
            BOOLINPUT,
            STRINGINPUT,
            SELECTOR
        }

        public enum NumberType
        {
            None,
            INT,
            LONG,
            FLOAT,
            DOUBLE,
            DECIMAL
        }

        public class ProcessId : Id<string>
        {
            public ProcessId(string value) : base(value)
            {
            }

            public new static ProcessId New => new ProcessId(Id<string>.New);
            public new static ProcessId FromString(string value) => new ProcessId(value);
            public new static ProcessId FromId(Id<string> value) => new ProcessId(value);
        }

        public class UserProcessId : Id<string>
        {
            public UserProcessId(string value) : base(value)
            {
            }

            public new static UserProcessId New => new UserProcessId(Id<string>.New);
            public new static UserProcessId FromString(string value) => new UserProcessId(value);
            public new static UserProcessId FromId(Id<string> value) => new UserProcessId(value);
        }

        public static class Models
        {
            /// <summary>_</summary>
            [Serializable]
            public class EditorModel
            {
                /// <summary>_</summary>
                /// <param name="editorId"></param>
                /// <param name="editorType"></param>
                /// <param name="numberType"></param>
                [JsonConstructor]
                public EditorModel(int editorId, EditorType editorType, NumberType numberType)
                {
                    EditorId = editorId;
                    EditorType = editorType;
                    NumberType = numberType;
                }

                [JsonProperty] public int EditorId { get; }

                [JsonProperty] public EditorType EditorType { get; }

                [JsonProperty] public NumberType NumberType { get; }

                [JsonProperty] public object Value { get; set; }

                public EditorModel SetValue(object value)
                {
                    Value = value;
                    return this;
                }

                public EditorModel FromJson(string value)
                {
                    return ImmutableJson.Deserialize<EditorModel>(value);
                }

                public override string ToString()
                    => JsonConvert.SerializeObject(this);
            }
        }

        public static class DomainEvents
        {
            public static class V1
            {
                // Config with attribute
                // [MessageConfig(
                //     RoutingKey="wizardapp.single_page_created_event",
                //     ExchangeType = ExchangeType.Topic,
                //     Durability = true,
                //     AutoDelete = false)]
                /// <summary> </summary>
                [Serializable]
                public class SingleProcessCreated : DomainEvent
                {
                    /// <summary> </summary>
                    /// <param name="aggregateId"></param>
                    /// <param name="valueModels"></param>
                    [JsonConstructor]
                    public SingleProcessCreated(
                        UserProcessId aggregateId,
                        IEnumerable<Models.EditorModel> valueModels)

                    {
                        SetAggregateId(aggregateId);
                        ValueModels = valueModels;
                    }

                    /// <summary></summary>
                    [JsonProperty]
                    public IEnumerable<Models.EditorModel> ValueModels { get; }
                }

                /// <summary></summary>
                public class UserProcessActivated : DomainEvent
                {
                    /// <summary></summary>
                    /// <param name="aggregateId"></param>
                    public UserProcessActivated(UserProcessId aggregateId)
                    {
                        SetAggregateId(aggregateId);
                        Active = true;
                    }

                    /// <summary></summary>
                    public bool Active { get; }
                }

                /// <summary> </summary>
                public class UserProcessCompleted : DomainEvent
                {
                    /// <summary> </summary>
                    /// <param name="aggregateId"></param>
                    /// <param name="value"></param>
                    public UserProcessCompleted(UserProcessId aggregateId, bool value)
                    {
                        SetAggregateId(aggregateId);
                        IsComplete = value;
                    }

                    /// <summary></summary>
                    public bool IsComplete { get; }
                }

                /// <summary></summary>
                public class UserProcessCreated : DomainEvent
                {
                    /// <summary> </summary>
                    /// <param name="processId"></param>
                    /// <param name="processVersion"></param>
                    /// <param name="userProcessId"></param>
                    /// <param name="userProcessName"></param>
                    /// <param name="userId"></param>
                    public UserProcessCreated(ProcessId processId,
                        int processVersion,
                        UserProcessId userProcessId,
                        string userProcessName,
                        UserId userId)
                    {
                        ProcessId = processId;
                        ProcessVersion = processVersion;
                        UserProcessName = userProcessName;
                        UserId = userId;
                        SetAggregateId(userProcessId);
                        IsClosed = false;
                    }

                    /// <summary> </summary>
                    public ProcessId ProcessId { get; }

                    /// <summary> </summary>
                    public int ProcessVersion { get; }

                    /// <summary> </summary>
                    public string UserProcessName { get; }

                    /// <summary> </summary>
                    public UserId UserId { get; }

                    /// <summary> </summary>
                    public bool IsClosed { get; }
                }

                /// <summary> </summary>
                public class UserProcessDeactivated : DomainEvent
                {
                    /// <summary></summary>
                    /// <param name="aggregateId"></param>
                    public UserProcessDeactivated(UserProcessId aggregateId)
                    {
                        SetAggregateId(aggregateId);
                        Active = false;
                    }

                    /// <summary> </summary>
                    public bool Active { get; }
                }

                /// <summary> </summary>
                public class UserStepFollowUpdated : DomainEvent
                {
                    /// <summary> </summary>
                    public UserStepFollowUpdated(UserProcessId aggregateId, int stepId)
                    {
                        SetAggregateId(aggregateId);
                        StepId = stepId;
                    }

                    /// <summary> </summary>
                    public int StepId { get; }
                }

                public class UserStepValuesUpdated : DomainEvent
                {
                    public IEnumerable<Models.EditorModel> ValueModels { get; }

                    public UserStepValuesUpdated(
                        UserProcessId aggregateId,
                        IEnumerable<Models.EditorModel> valueModels)
                    {
                        SetAggregateId(aggregateId);
                        ValueModels = valueModels;
                    }
                }
            }
        }
    }
}