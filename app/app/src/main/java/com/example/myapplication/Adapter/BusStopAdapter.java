package com.example.myapplication.Adapter;

import android.content.Context;
import android.content.Intent;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.example.myapplication.Activity.ScheduleLineActivity;
import com.example.myapplication.Item.BusStopItem;
import com.example.myapplication.Activity.BusStopScheduleActivity;
import com.example.myapplication.R;

import java.util.ArrayList;
//Adapter for the BusStopItem
public class BusStopAdapter extends ArrayAdapter<BusStopItem> {

    private Context mContext;
    public BusStopAdapter(@NonNull Context context, ArrayList<BusStopItem> dataArrayList) {
        super(context, R.layout.bus_stop_list,dataArrayList);
        mContext = context;
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

        nameTxt.setText(busStopItem.getName());
        cityTxt.setText(busStopItem.getCity());
        //on click to go on BusStopScheduleActivity with the transfer of some information
        view.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Intent intent = new Intent(mContext, BusStopScheduleActivity.class);
                intent.putExtra("line", ((ScheduleLineActivity)mContext).getLine());
                intent.putExtra("busStop",busStopItem);
                intent.putExtra("aller", ((ScheduleLineActivity)mContext).getSens());
                mContext.startActivity(intent);
            }
        });

        return view;
    }
}
