﻿//===============================================================================
// Microsoft patterns & practices
// Unity Application Block
//===============================================================================
// Copyright © Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Microsoft.Practices.Unity.Properties;
using Microsoft.Practices.Unity.Utility;

namespace Microsoft.Practices.Unity
{
    /// <summary>
    /// The exception thrown by the Unity container when
    /// an attempt to resolve a dependency fails.
    /// </summary>
    // FxCop suppression: The standard constructors don't make sense for this exception,
    // as calling them will leave out the information that makes the exception useful
    // in the first place.
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    public partial class ResolutionFailedException : Exception
    {
        private readonly string typeRequested;
        private readonly string nameRequested;

        /// <summary>
        /// Create a new <see cref="ResolutionFailedException"/> that records
        /// the exception for the given type and name.
        /// </summary>
        /// <param name="typeRequested">Type requested from the container.</param>
        /// <param name="nameRequested">Name requested from the container.</param>
        /// <param name="innerException">The actual exception that caused the failure of the build.</param>
        public ResolutionFailedException(Type typeRequested, string nameRequested, Exception innerException) 
            : base(CreateMessage(typeRequested, nameRequested, innerException), innerException)
        {
            if (typeRequested != null)
            {
                this.typeRequested = typeRequested.Name;
            }
            this.nameRequested = nameRequested;
        }

        /// <summary>
        /// The type that was being requested from the container at the time of failure.
        /// </summary>
        public string TypeRequested
        {
            get { return typeRequested; }
        }

        /// <summary>
        /// The name that was being requested from the container at the time of failure.
        /// </summary>
        public string NameRequested
        {
            get { return nameRequested; }
        }

        private static string CreateMessage(Type typeRequested, string nameRequested, Exception innerException)
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                Resources.ResolutionFailed,
                typeRequested,
                nameRequested,
                innerException != null ? innerException.Message : null);
        }
    }
}