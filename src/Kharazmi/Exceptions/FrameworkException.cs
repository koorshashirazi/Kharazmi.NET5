#region

using System;
using System.Runtime.Serialization;
using Kharazmi.Helpers;

#endregion

namespace Kharazmi.Exceptions
{
    [Serializable]
    public class FrameworkException : Exception, IDisposable
    {
        private bool _isDisposed;

        /// <summary> </summary>
        public string? CreateAt { get; }

        /// <summary> </summary>
        public string? ExceptionId { get; }

        /// <summary> </summary>
        public string? Code { get; protected set; }

        /// <summary> </summary>
        public string? Description { get; protected set; }


        /// <summary>_</summary>
        public FrameworkException()
            : this("")
        {
        }

        /// <summary>_</summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public FrameworkException(string? message, Exception? innerException)
            : this(message, innerException, "")
        {
        }

        /// <summary>_</summary>
        /// <param name="serializationInfo"></param>
        /// <param name="context"></param>
        protected FrameworkException(SerializationInfo serializationInfo, StreamingContext context)
            : base(serializationInfo, context)
        {
        }


        /// <summary>_</summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        /// <param name="description"></param>
        /// <param name="code"></param>
        public FrameworkException(string? message, Exception? innerException = null, string? description = "",
            string? code = "") : base(message, innerException)
        {
            Code = code;
            Description = description;
            CreateAt = DateTimeHelper.DateTimeOffsetUtcNow.UtcDateTime.ToString("g");
            ExceptionId = Guid.NewGuid().ToString("N");
        }

        /// <summary>_</summary>
        /// <param name="description"></param>
        /// <returns></returns>
        public FrameworkException WithDescription(string? description)
        {
            Description = description;
            return this;
        }

        /// <summary>_</summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public FrameworkException WithCode(string? code)
        {
            Code = code;
            return this;
        }


        /// <summary>
        /// free managed resources
        /// </summary>
        public virtual void Clear()
        {
        }

        /// <summary>
        /// free native resources if there are any.
        /// </summary>
        public virtual void NativeResourceClear()
        {
        }

        /// <summary>
        /// Dispose() calls Dispose(true)
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///  The bulk of the clean-up code is implemented in Dispose(bool)
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
                Clear();

            NativeResourceClear();

            _isDisposed = true;
        }


        /// <summary>
        /// NOTE: Leave out the finalizer altogether if this class doesn't
        /// own unmanaged resources, but leave the other methods
        /// exactly as they are.
        /// </summary>
        ~FrameworkException()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }
    }
}