package com.example.myapplication.Adapter;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.ImageView;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.example.myapplication.Item.BusStopItem;
import com.example.myapplication.Item.LineList;
import com.example.myapplication.Item.RoadInfoItem;
import com.example.myapplication.R;

import java.util.ArrayList;
//Adapter for the road between 2 bus stop
public class RoadInfoAdapter extends ArrayAdapter<RoadInfoItem> {

    public RoadInfoAdapter(@NonNull Context context, ArrayList<RoadInfoItem> dataArrayList) {
        super(context, R.layout.road_info_layout,dataArrayList);
    }

    @NonNull
    @Override
    public View getView(int position, @Nullable View view, @NonNull ViewGroup parent) {
        RoadInfoItem roadInfoItem = getItem(position);

        if(view == null){
            view = LayoutInflater.from(getContext()).inflate(R.layout.road_info_layout,parent,false);
        }
        ImageView imageView = view.findViewById(R.id.imageLineRoad);
        //get the number of the line and choose the right logo
        switch (roadInfoItem.getLine()){
            case "1":
                imageView.setBackgroundResource(R.drawable.line1);
                break;
            case "2":
                imageView.setBackgroundResource(R.drawable.line2);
                break;
            case "3":
                imageView.setBackgroundResource(R.drawable.line3);
                break;
            case "4":
                imageView.setBackgroundResource(R.drawable.line4);
                break;
            case "5":
                imageView.setBackgroundResource(R.drawable.line5);
                break;
            case "6":
                imageView.setBackgroundResource(R.drawable.line6);
                break;
            default:
                imageView.setBackgroundResource(R.drawable.line6);
                break;
        }
        //display the informations
        TextView departTxt = view.findViewById(R.id.textDepart);
        TextView arriveeTxt = view.findViewById(R.id.textArrival);
        TextView minuteToGo = view.findViewById(R.id.minuteTogo);

        departTxt.setText(roadInfoItem.getDeparture());
        arriveeTxt.setText(roadInfoItem.getArrival());
        minuteToGo.setText(roadInfoItem.getTimeLeft() +" min");

        return view;
    }
}
