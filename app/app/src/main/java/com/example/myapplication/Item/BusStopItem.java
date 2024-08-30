package com.example.myapplication.Item;

import android.os.Parcel;
import android.os.Parcelable;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

//This is the class for the BusStop we will retrieve from the API
public class BusStopItem implements Parcelable {

    private String name;
    private String city;
    //Schedules of the stop for the first direction of the line
    private HashMap<String,List<String>> schedulesAller;
    //Schedules of the stop for the opposite direction of the line
    private HashMap<String,List<String>> schedulesRetour;
    public BusStopItem(String name, String city) {
        this.name = name;
        this.city = city;
        schedulesAller = new HashMap<>();
        schedulesRetour = new HashMap<>();
    }
    public BusStopItem(String name, String city,HashMap<String,List<String>> schedules) {
        this.name = name;
        this.city = city;
        this.schedulesAller = schedules;
        schedulesRetour = new HashMap<>();
    }
    public BusStopItem(String name, String city,HashMap<String,List<String>> schedulesA,HashMap<String,List<String>> schedulesR) {
        this.name = name;
        this.city = city;
        this.schedulesAller = schedulesA;
        this.schedulesRetour = schedulesR;
    }
    //Parcelable constructor
    protected BusStopItem(Parcel in) {
        name = in.readString();
        city = in.readString();
        schedulesAller = (HashMap<String, List<String>>) in.readSerializable();
        schedulesRetour = (HashMap<String, List<String>>) in.readSerializable();
    }

    public static final Creator<BusStopItem> CREATOR = new Creator<BusStopItem>() {
        @Override
        public BusStopItem createFromParcel(Parcel in) {
            return new BusStopItem(in);
        }

        @Override
        public BusStopItem[] newArray(int size) {
            return new BusStopItem[size];
        }
    };

    public String getName() {
        return name;
    }

    public void setName(String name) {
        this.name = name;
    }

    public String getCity() {
        return city;
    }

    public void setCity(String city) {
        this.city = city;
    }
    //An equals function just for the name of the bus stop
    @Override
    public boolean equals(@Nullable Object obj) {
        if (this == obj) return true;
        if (obj == null || getClass() != obj.getClass()) return false;

        BusStopItem station = (BusStopItem) obj;

        return name != null ? name.equals(station.name) : station.name == null;
    }

    public HashMap<String, List<String>> getSchedulesAller() {
        return schedulesAller;
    }

    public void setSchedulesAller(HashMap<String, List<String>> schedulesAller) {
        this.schedulesAller = schedulesAller;
    }

    public HashMap<String, List<String>> getSchedulesRetour() {
        return schedulesRetour;
    }

    public void setSchedulesRetour(HashMap<String, List<String>> schedulesRetour) {
        this.schedulesRetour = schedulesRetour;
    }

    @Override
    public int describeContents() {
        return 0;
    }

    @Override
    public void writeToParcel(Parcel dest, int flags) {
        dest.writeString(name);
        dest.writeString(city);
        dest.writeSerializable(schedulesAller);
        dest.writeSerializable(schedulesRetour);
    }
}
