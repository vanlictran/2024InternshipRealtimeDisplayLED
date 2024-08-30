package com.example.myapplication.Adapter;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.example.myapplication.R;

import java.util.List;
//Adapter for the minutes in the schedule of a bus stop
public class MinuteAdapter extends ArrayAdapter<String> {

    public MinuteAdapter(@NonNull Context context, List<String> dataArrayList) {
        super(context, R.layout.minute_list, dataArrayList);
    }

    @NonNull
    @Override
    public View getView(int position, @Nullable View convertView, @NonNull ViewGroup parent) {
        ViewHolder holder;
        //create the view if it's null
        if (convertView == null) {
            convertView = LayoutInflater.from(getContext()).inflate(R.layout.minute_list, parent, false);
            holder = new ViewHolder();
            holder.minuteTextView = convertView.findViewById(R.id.minuteItem);
            convertView.setTag(holder);
        } else {
            holder = (ViewHolder) convertView.getTag();
        }
        //create the string with all the minutes digit retrieved
        boolean ok = true;
        String s = "";
        int i = 0;
        while (ok){
            String tmp = null;
            try{
                tmp = getItem(i);
            }catch (Exception e){
                ok = false;
            }
            if(tmp ==null){
                ok = false;
            }else{
                s += tmp+"   ";
            }
            i++;


        }
        if (s != null) {
            holder.minuteTextView.setText(s);
        }

        return convertView;
    }

    static class ViewHolder {
        TextView minuteTextView;
    }
}
