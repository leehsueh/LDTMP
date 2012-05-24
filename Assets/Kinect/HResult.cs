//-----------------------------------------------------------------------------
// HResult.cs
//
// Represents the Win32 HRESULT return value
//
// Copyright (C) Microsoft. All rights reserved.
//-----------------------------------------------------------------------------

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;


/// <summary>
/// Represents the Win32 HRESULT return value
/// Normal use should be to just use the Succeeded or Failed properties
/// 
/// HRESULTs are 32 bit values layed out as follows:
/// 
///  3 3 2 2 2 2 2 2 2 2 2 2 1 1 1 1 1 1 1 1 1 1
///  1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0
/// +-+-+-+-+-+---------------------+-------------------------------+
/// |S|R|C|N|r|    Facility         |               Code            |
/// +-+-+-+-+-+---------------------+-------------------------------+
/// 
/// where
/// 
///     S - Severity - indicates success/fail
/// 
///         0 - Success
///         1 - Fail (COERROR)
/// 
///     R - reserved portion of the facility code, corresponds to NT's
///             second severity bit.
/// 
///     C - reserved portion of the facility code, corresponds to NT's
///             C field.
/// 
///     N - reserved portion of the facility code. Used to indicate a
///             mapped NT status value.
/// 
///     r - reserved portion of the facility code. Reserved for internal
///             use. Used to indicate HRESULT values that are not status
///             values, but are instead message ids for display strings.
/// 
///     Facility - is the facility code
/// 
///     Code - is the facility's status code
/// 
/// </summary>
public struct HRESULT
{
    /// <summary>
    /// internal value of the result 
    /// </summary>
    private uint Value;
	
    /// <summary>
    /// succeeded 
    /// </summary>
	public const uint S_OK = 0;
	
    /// <summary>
    /// alternate succeeded where a hint of no action was done (use rarely)
    /// </summary>
    public const uint S_FALSE = 1;
	
    /// <summary>
    /// Catastrophic failure
    /// </summary>
    public const uint E_UNEXPECTED = 0x8000FFFF;
	
    /// <summary>
    /// Not implemented
    /// </summary>
    public const uint E_NOTIMPL = 0x80004001;
	
    /// <summary>
    /// Ran out of memory
    /// </summary>
    public const uint E_OUTOFMEMORY = 0x8007000E;
	
    /// <summary>
    /// One or more arguments are invalid
    /// </summary>
    public const uint E_INVALIDARG = 0x80070057;
	
    /// <summary>
    /// No such interface supported
    /// </summary>
    public const uint E_NOINTERFACE = 0x80004002;
	
    /// <summary>
    /// Invalid pointer, often refered to a pointer specific argument
    /// </summary>
    public const uint E_POINTER = 0x80004003;
	
    /// <summary>
    /// Invalid handle
    /// </summary>
    public const uint E_HANDLE = 0x80070006;
	
    /// <summary>
    /// Operation aborted
    /// </summary>
    public const uint E_ABORT = 0x80004004;
	
    /// <summary>
    /// Unspecified error, most commonly used for general errors
    /// </summary>
    public const uint E_FAIL = 0x80004005;
	
    /// <summary>
    /// General access denied error
    /// </summary>
    public const uint E_ACCESSDENIED = 0x80070005;
	
    /// <summary>
    /// The data necessary to complete this operation is not yet available
    /// </summary>
    public const uint E_PENDING = 0x8000000A;
	
	/// <summary>
	/// No data for the next frame is yet available
	/// </summary>
	public const uint E_NUI_FRAME_NO_DATA = 0x83010001;

    /// <summary>
    /// See class summary for bit descriptions
    /// </summary>
    private const uint SeverityMask = 0x80000000;
    private const uint FacilityMask = 0x1fff0000;
    private const int FacilityShift = 16;
    private const uint CodeMask = 0x0000ffff;

    /// <summary>
    /// Gets if this represents a succeeded state
    /// </summary>
    public bool Succeeded
    {
        get
        {
            return (this.Value & SeverityMask) == 0;
        }
    }

    /// <summary>
    /// Gets if this represents a failed state
    /// </summary>
    public bool Failed
    {
        get
        {
            return !this.Succeeded;
        }
    }

    /// <summary>
    /// Gets the facility code (origin of the result)
    /// this should be rarely used and useful primarily for debugging
    /// see WinError.h from the Windows SDK for mapping
    /// </summary>
    public uint Facility
    {
        get
        {
            return (this.Value & FacilityMask) >> FacilityShift;
        }
    }

    /// <summary>
    /// Gets the specific code of this error
    /// this should be rarely used and useful primarily for debugging
    /// </summary>
    public uint Code
    {
        get
        {
            return this.Value & CodeMask;
        }
    }
	
	/// <summary>
    /// Implicit conversion of uint to HRESULT, required as using HRESULT for the return
    /// type for the pInvoked functions causes undesired behavior on the Xbox
    /// </summary>
    /// <param name="hr">The HRESULT to test its value</param>
    /// <returns>HRESULT struct containing the value hr</returns>
	public static implicit operator HRESULT(uint hr)
	{
		HRESULT HR = new HRESULT();
		HR.Value = hr;
		return HR;
	}
	
    /// <summary>
    /// Check if the HRESULT represents the given hresult
    /// </summary>
    /// <param name="hr">The HRESULT to test its value</param>
    /// <param name="hresult">the uint hresult value</param>
    /// <returns>true if this HRESULT represents the given hresult</returns>
    public static bool operator==(HRESULT hr, uint hresult)
    {
        return (hr.Value == hresult);
    }
	
	/// <summary>
    /// Check if the HRESULT does not represents the given hresult
    /// </summary>
    /// <param name="hr">The HRESULT to test its value</param>
    /// <param name="hresult">the uint hresult value</param>
    /// <returns>true if this HRESULT does not represents the given hresult</returns>
    public static bool operator!=(HRESULT hr, uint hresult)
    {
        return (hr.Value != hresult);
    }

    /// <summary>
    /// Check if the HRESULT represents the given hresult
    /// </summary>
    /// <param name="hresult">the uint hresult value</param>
    /// <param name="hr">The HRESULT to test its value</param>/// 
    /// <returns>true if this HRESULT represents the given hresult</returns>
    public static bool operator==(uint hresult, HRESULT hr)
    {
        return (hr == hresult);
    }
	
	/// <summary>
    /// Check if the HRESULT does not represents the given hresult
    /// </summary>
    /// <param name="hresult">the uint hresult value</param>
    /// <param name="hr">The HRESULT to test its value</param>/// 
    /// <returns>true if this HRESULT does not represents the given hresult</returns>
    public static bool operator!=(uint hresult, HRESULT hr)
    {
        return (hr != hresult);
    }

	/// <summary>
	/// Helper method to throw a standard set of exception depending on the value of HRESULT 
	/// </summary>
	/// <param name="hresult">
	/// The HRESULT to test <see cref="HRESULT"/>
	/// </param>
	/// <returns>
	/// The same HRESULT as passed in <see cref="HRESULT"/>
	/// </returns>
	static public HRESULT ThrowOnFailure( HRESULT hresult )
	{
		if (hresult.Failed)
		{
			Exception exception;
			switch (hresult.Value)
			{
			case E_NOTIMPL:
				exception = new System.NotImplementedException();
				break;
			case E_OUTOFMEMORY:
				exception = new System.OutOfMemoryException();
				break;
			case E_POINTER:
 				exception = new System.ArgumentNullException();
				break;
			case E_INVALIDARG:
				exception = new System.ArgumentException();
				break;
			case E_ACCESSDENIED:
				exception = new System.UnauthorizedAccessException();
				break;
			case E_UNEXPECTED:
				exception = new System.Exception( "Unexpected" );
				break;
			case E_NOINTERFACE:
				exception = new System.Exception( "Interface not supported." );
				break;
			case E_HANDLE:
				exception = new System.Exception( "Invalid Handle." );
				break;
			case E_FAIL:
				exception = new System.Exception( "General failure." );
				break;
			case E_ABORT:
				exception = new System.Exception( "Operation aborted." );
				break;
			case E_PENDING:
				exception = new System.Exception( "Operation pending." );
				break;
			case E_NUI_FRAME_NO_DATA:
				exception = new System.Exception( "No frame data available" );
				break;
			default:
				exception = new System.Exception( String.Format( "HRESULT. ({0:x8})", hresult.Value) );
				break;
			}
			throw exception;
		}
		return hresult;
	}
	
}


