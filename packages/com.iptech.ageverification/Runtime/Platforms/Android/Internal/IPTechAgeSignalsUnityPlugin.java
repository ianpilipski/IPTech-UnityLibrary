package com.IPTech.AgeVerification.Android.agesignals;

import android.app.Activity;

import com.google.android.play.agesignals.AgeSignalsAccessRequest;
import com.google.android.play.agesignals.AgeSignalsAccessResult;
import com.google.android.play.agesignals.AgeSignalsException;
import com.google.android.play.agesignals.AgeSignalsManager;
import com.google.android.play.agesignals.AgeSignalsManagerFactory;
import com.google.android.play.agesignals.AgeSignalsRequest;
import com.google.android.play.agesignals.AgeSignalsResult;
import com.unity3d.player.UnityPlayer;

public class IPTechAgeSignalsUnityPlugin
{
    public interface ICheckAgeSignalsCallback {
        void onSuccess(AgeSignalsResult result);
        void onFailure(int errorCode, String errorMsg);
        void onCancel();
    }

    public interface IRequestAgeSignalsAccessCallback {
        void onSuccess(AgeSignalsAccessResult ageSignalsRequestStatus);
        void onFailure(int errorCode, String errorMsg);
        void onCancel();
    }

    private static AgeSignalsManager ageSignalsManagerInst;
    private static AgeSignalsManager getAgeSignalsManager() throws IllegalStateException {
        if (ageSignalsManagerInst == null) {
            // create the age signals manager instance
            // get the current activity
            Activity currentActivity = UnityPlayer.currentActivity;
            if (currentActivity == null) {
                throw new IllegalStateException("Could not get the UnityPlayer.currentActivity");
            }

            // Create an instance of a manager
            ageSignalsManagerInst = AgeSignalsManagerFactory.create(currentActivity.getApplicationContext());
        }
        return ageSignalsManagerInst;
    }

    public static void requestAgeSignalsAccess(IRequestAgeSignalsAccessCallback callback) {
        try {
            AgeSignalsManager ageSignalsManager = getAgeSignalsManager();

            // 2. Request or check for age signals access.
            // Passing the current Activity allows the Play Store to render the age sharing prompt UI if required.
            AgeSignalsAccessRequest accessRequest = AgeSignalsAccessRequest.builder()
                    .setActivity(UnityPlayer.currentActivity)
                    .build();

            ageSignalsManager.requestAgeSignalsAccess(accessRequest)
                    .addOnSuccessListener(callback::onSuccess)
                    .addOnFailureListener( exception -> {
                        HandleRequestAgeSignalAccessException(callback, exception);
                    })
                    .addOnCanceledListener(callback::onCancel);
        }
        catch(Exception e) {
            HandleRequestAgeSignalAccessException(callback, e);
        }
    }

    public static void checkAgeSignals(ICheckAgeSignalsCallback callback) {
        try {
            AgeSignalsManager ageSignalsManager = getAgeSignalsManager();

            ageSignalsManager.checkAgeSignals(AgeSignalsRequest.builder().build())
                    .addOnSuccessListener(callback::onSuccess)
                    .addOnFailureListener(exception -> {
                        HandleCheckAgeSignalsException(callback, exception);
                    })
                    .addOnCanceledListener(callback::onCancel);
        }
        catch(Exception e) {
            HandleCheckAgeSignalsException(callback, e);
        }
    }

    private static void HandleRequestAgeSignalAccessException(IRequestAgeSignalsAccessCallback callback, Exception e) {
        if (e instanceof AgeSignalsException) {
            AgeSignalsException ase = (AgeSignalsException)e;
            callback.onFailure(ase.getErrorCode(), ase.getMessage());
        } else {
            callback.onFailure(0, e.getMessage());
        }
    }

    private static void HandleCheckAgeSignalsException(ICheckAgeSignalsCallback callback, Exception e)
    {
        if(e instanceof AgeSignalsException) {
            AgeSignalsException ase = (AgeSignalsException)e;
            callback.onFailure(ase.getErrorCode(), ase.getMessage());
        } else {
            callback.onFailure(0, e.getMessage());
        }
    }
}
