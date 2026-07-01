package com.IPTech.AgeVerification.Android.agesignals;

import android.app.Activity;

import com.google.android.play.agesignals.AgeSignalsException;
import com.google.android.play.agesignals.AgeSignalsManager;
import com.google.android.play.agesignals.AgeSignalsManagerFactory;
import com.google.android.play.agesignals.AgeSignalsRequest;
import com.google.android.play.agesignals.AgeSignalsResult;
import com.unity3d.player.UnityPlayer;

public class IPTechAgeSignalsUnityPlugin
{
    public interface IRequestAgeSignalsCallback {
        void onSuccess(AgeSignalsResult result);
        void onFailure(int errorCode, String errorMsg);
        void onCancel();
    }

    public static void requestAgeSignals(IRequestAgeSignalsCallback callback) {
        try {
            // get the current activity
            Activity currentActivity = UnityPlayer.currentActivity;
            if (currentActivity == null) {
                callback.onFailure(0, "Could not get the UnityPlayer.currentActivity");
                return;
            }

            // Create an instance of a manager
            AgeSignalsManager ageSignalsManager = AgeSignalsManagerFactory.create(currentActivity.getApplicationContext());

            // Request an age signals check
            ageSignalsManager
                    .checkAgeSignals(AgeSignalsRequest.builder().build())
                    .addOnSuccessListener(callback::onSuccess)
                    .addOnFailureListener(e -> {
                        HandleException(callback, e);
                    })
                    .addOnCanceledListener(callback::onCancel);
        }
        catch(Exception e) {
            HandleException(callback, e);
        }
    }

    private static void HandleException(IRequestAgeSignalsCallback callback, Exception e) {
        if (e instanceof AgeSignalsException) {
            AgeSignalsException ase = (AgeSignalsException)e;
            callback.onFailure(ase.getErrorCode(), ase.getMessage());
        } else {
            callback.onFailure(0, e.getMessage());
        }
    }
}
