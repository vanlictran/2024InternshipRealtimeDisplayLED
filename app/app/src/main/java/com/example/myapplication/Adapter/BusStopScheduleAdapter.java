package com.example.myapplication.Adapter;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.ListView;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.example.myapplication.R;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.TreeMap;
//Adapter for the schedule of a bus stop
public class BusStopScheduleAdapter extends ArrayAdapter<Map.Entry<Integer, List<String>>> {

    private Context mContext;
    private int mResource;


    public BusStopScheduleAdapter(@NonNull Context context, int resource, @NonNull TreeMap<Integer, List<String>> hashMap) {
        super(context, resource, new ArrayList<>(hashMap.entrySet()));
        this.mContext = context;
        this.mResource = resource;
    }

    @NonNull
    @Override
    public View getView(int position, @Nullable View convertView, @NonNull ViewGroup parent) {
        ViewHolder holder;
        //create the view if it's null
        if(convertView == null){
            convertView = LayoutInflater.from(getContext()).inflate(R.layout.hour_list, parent, false);
            holder = new ViewHolder();
            holder.keyTextView = convertView.findViewById(R.id.hourBusStop);
            holder.minutesListView = convertView.findViewById(R.id.listMinute);
            convertView.setTag(holder);
        } else {
            holder = (ViewHolder) convertView.getTag();
        }

        Map.Entry<Integer, List<String>> entry = getItem(position);
       //display the hour digit and call the other adpater for the minutes
        if (entry != null) {
            holder.keyTextView.setText(""+entry.getKey());
            MinuteAdapter minuteAdapter = new MinuteAdapter(mContext, entry.getValue());
            holder.minutesListView.setAdapter(minuteAdapter);
        }

        return convertView;
    }

    static class ViewHolder {
        TextView keyTextView;
        ListView minutesListView;
    }
}
