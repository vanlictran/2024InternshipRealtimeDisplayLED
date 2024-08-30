package com.example.myapplication.Adapter;

import android.content.Context;
import android.graphics.Color;
import android.os.Build;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.annotation.RequiresApi;

import com.example.myapplication.Activity.SearchRoadActivity;
import com.example.myapplication.Item.BusStopItem;
import com.example.myapplication.R;

import java.util.ArrayList;
//Adapter for the searchRoadActivity with the list of the BusStopItem
public class SearchRoadAdapter extends ArrayAdapter<BusStopItem> {
    private Context mContext;
    private TextView departure;
    private TextView arrival;
    private BusStopItem busStopItem1;
    private BusStopItem busStopItem2;
    public SearchRoadAdapter(@NonNull Context context, ArrayList<BusStopItem> dataArrayList,TextView departure,TextView arrival) {
        super(context, R.layout.bus_stop_list,dataArrayList);
        mContext = context;
        this.departure = departure;
        this.arrival = arrival;
    }
    @NonNull
    @Override
    public View getView(int position, @Nullable View view, @NonNull ViewGroup parent) {
        BusStopItem busStopItem = getItem(position);

        if(view == null){
            view = LayoutInflater.from(getContext()).inflate(R.layout.bus_stop_list,parent,false);
        }
        //display the informations
        TextView nameTxt = view.findViewById(R.id.busStopName);
        TextView cityTxt = view.findViewById(R.id.cityStopName);

        nameTxt.setText( busStopItem.getName());
        cityTxt.setText( busStopItem.getCity());
        //on click on the item to add it on the departure/arrival textView
        //and the item is remove form the list
        view.setOnClickListener(new View.OnClickListener() {
            @RequiresApi(api = Build.VERSION_CODES.O)
            @Override
            public void onClick(View v) {

                if(departure.getText().equals("where do you want to start...")){
                    departure.setText(busStopItem.getName());
                    departure.setTextColor(Color.parseColor("#000000"));
                    busStopItem1 = busStopItem;
                }else{
                    arrival.setText(busStopItem.getName());
                    arrival.setTextColor(Color.parseColor("#000000"));
                    busStopItem2 = busStopItem;
                    ((SearchRoadActivity) mContext).findRoads(busStopItem1,busStopItem2);
                }
                remove(busStopItem);
                // Notify the adapter that the list has changed
                notifyDataSetChanged();
            }
        });

        return view;
    }
}