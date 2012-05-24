//-----------------------------------------------------------------------------
// KinectFilter.cs
//
// Manages the Kinect interface via Gak2Unity for Filters
// Will become a base class for future filters to derive from
//
// Copyright (C) Microsoft. All rights reserved.
//-----------------------------------------------------------------------------

using UnityEngine;
using System.Collections;

using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;

/// <summary>
/// KinectFilter represents a directly usable base class for Gak filters
/// For simple filters it can be used directly
/// For more complex filters (ones that use SetFilterSettings and GetFilterOutput)
/// they will derive from this class and expose wrapper properties and methods for
/// settings and data access
/// 
/// This class is not thread safe and that protection is left up to the consumer
/// Filters must be created in priority order
/// </summary>
public class KinectFilter : IDisposable
{
    /// <summary>
    /// CurrentPriority the next Filter will get added as
    /// </summary>
    static float s_CurrentPriority = 0.0f;

    /// <summary>
    /// The amount the priority will increment for every filter add
    /// </summary>
    const float c_PriorityIncrement = 0.01f;

    /// <summary>
    /// opaque unmanaged handle to the filter exposed through Gak
    /// </summary>
    uint m_handle;

    /// <summary>
    /// unmanaged dispose state 
    /// </summary>
    bool m_isDisposed;

    /// <summary>
    /// Create a Filter identified by the given fuid and with the given starting properties
    /// </summary>
    /// <param name="filterGuid">The guid identifier of the the filter to create</param>
    /// <param name="isEnabled">The starting state of the filter</param>
    public KinectFilter(Guid filterFuid, bool isEnabled)
    {
        HRESULT.ThrowOnFailure(KinectInterface.GakAddFilter(
                filterFuid.ToString(),
                isEnabled,
                s_CurrentPriority,
                out this.m_handle));
        s_CurrentPriority += c_PriorityIncrement;
    }

    /// <summary>
    /// Finializer for the filter, used for unmanaged content management
    /// </summary>
    ~KinectFilter()
    {
        Dispose(false);
    }

    /// <summary>
    /// IDisposable.Dispose, used for unmanaged content management
    /// </summary>
    public void Dispose()
    {
        Remove();
    }

    /// <summary>
    /// Remove the filter from active use
    /// </summary>
    public void Remove()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// virtual Dispose for when this class is derived from
    /// The derived class can use this to manage its specific unmanaged/managed content
    /// This method is overly commented to provide a template for others when they derive
    /// </summary>
    /// <param name="isDisposing">is the class being being forcefully disposed</param>
    protected virtual void Dispose(bool isDisposing)
    {
        if (!this.m_isDisposed)
        {
            if (isDisposing)
            {
                // release managed resources here
            }

            // release unmanaged resources here
            if (this.m_handle != 0)
            {
                // ignore return, we can't throw exceptions during a Dispose
                KinectInterface.GakRemoveFilter(this.m_handle);
                this.m_handle = 0;
            }
        }
        // call base class if there is a need
        // base.Dispose( isDisposing );
        this.m_isDisposed = true; // only base class should set this
    }

    /// <summary>
    /// Gets or Sets if the current filter is enabled
    /// 
    /// Note that this method is not lightweight and should not be used
    /// in tight loops like the update loop
    /// </summary>
    public bool IsEnabled
    {
        get
        {
            if (this.m_isDisposed)
            {
                throw new System.ObjectDisposedException("KinectFilter");
            }
            bool isEnabled;
            HRESULT.ThrowOnFailure(KinectInterface.GakGetFilterEnabled(
                this.m_handle,
                out isEnabled));
            return isEnabled;
        }
        set
        {
            if (this.m_isDisposed)
            {
                throw new System.ObjectDisposedException("KinectFilter");
            }
            HRESULT.ThrowOnFailure(KinectInterface.GakSetFilterEnabled(
                this.m_handle,
                value));
        }
    }
}
