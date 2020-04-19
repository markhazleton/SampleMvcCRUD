// ***********************************************************************
// Assembly         : Mwh.Sample.Common
// Author           : mark
// Created          : 04-05-2020
//
// Last Modified By : mark
// Last Modified On : 04-05-2020
// ***********************************************************************
// <copyright file="BaseResponse.cs" company="Mark Hazleton">
//     Copyright 2020 Mark Hazleton
// </copyright>
// <summary></summary>
// ***********************************************************************
namespace Mwh.Sample.Common.Repositories
{
    /// <summary>
    /// Class BaseResponse.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseResponse<T>
    {
        /// <summary>
        /// Gets a value indicating whether this <see cref="BaseResponse{T}"/> is success.
        /// </summary>
        /// <value><c>true</c> if success; otherwise, <c>false</c>.</value>
        public bool Success { get; private set; }
        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <value>The message.</value>
        public string Message { get; private set; }
        /// <summary>
        /// Gets the resource.
        /// </summary>
        /// <value>The resource.</value>
        public T Resource { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseResponse{T}"/> class.
        /// </summary>
        /// <param name="resource">The resource.</param>
        protected BaseResponse(T resource)
        {
            Success = true;
            Message = string.Empty;
            Resource = resource;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseResponse{T}"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        protected BaseResponse(string message)
        {
            Success = false;
            Message = message;
            Resource = default;
        }
    }
}
