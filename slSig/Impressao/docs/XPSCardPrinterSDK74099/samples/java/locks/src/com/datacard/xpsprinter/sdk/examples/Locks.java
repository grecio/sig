////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////
package com.datacard.xpsprinter.sdk.examples;

import common_java.XPS_Java_SDK.XpsDriverInteropLib;

public class Locks {
    private String mPrinterName;

    public Locks(String aPrinterName) {
        mPrinterName = aPrinterName;
    }

    public void ActivatePrinter(boolean activate, String password) {
        int S_OK = 0;
        long hResult = -1; // not S_OK

        // call driver to set activate or deactivate printer (job queue)
        hResult = XpsDriverInteropLib.INSTANCE.ActivateOrDisablePrinter(mPrinterName, activate, password);

        if (S_OK == hResult) {
            System.out.format("'%s' ActivatePrinter: %s, success", mPrinterName,
                    (activate == true) ? "Activate" : "Deactivate");
        } else {
            System.out.format("'%s' ActivatePrinter: %s, failed, hResult = %d", mPrinterName,
                    (activate == true) ? "Activate" : "Deactivate", hResult);
        }

    }

    public void SetLockState(int lockState, String lockPassword) {
        int S_OK = 0;
        long hResult = -1; // not S_OK

        // call driver to set printer lock state
        hResult = XpsDriverInteropLib.INSTANCE.SetPrinterLockState(mPrinterName, lockState, lockPassword);

        if (S_OK == hResult) {
            System.out.format("'%s' SetPrinterLockState: %s, success", mPrinterName,
                    (lockState == 1) ? "Locked" : "Unlocked");
        } else {
            System.out.format("'%s' SetPrinterLockState: %s, failed, hResult = %d", mPrinterName,
                    (lockState == 1) ? "Locked" : "Unlocked", hResult);
        }

    }

    public void ChangePassword(String currentPassword, String nextPassword) {
        int S_OK = 0;
        long hResult = -1; // not S_OK

        // in this sample, we always lock the printer as part of the password change
        // operation. (Use zero to unlock the printer.)
        int lockValue = 1;

        // call driver to change lock password
        hResult = XpsDriverInteropLib.INSTANCE.ChangeLockPassword(mPrinterName, lockValue, currentPassword,
                nextPassword);

        if (S_OK == hResult) {
            System.out.format("'%s' ChangeLockPassword: success", mPrinterName);
        } else {
            System.out.format("'%s' ChangeLockPassword: failed, hResult = %d", mPrinterName, hResult);
        }
    }

}
