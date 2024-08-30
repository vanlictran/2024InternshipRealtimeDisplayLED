package com.example.myapplication.Activity;

import android.content.Intent;
import android.graphics.Color;
import android.os.Bundle;
import android.view.View;
import android.widget.ImageView;
import android.widget.ListView;
import android.widget.TextView;

import androidx.annotation.Nullable;
import androidx.appcompat.app.AppCompatActivity;

import com.example.myapplication.Adapter.BusStopAdapter;
import com.example.myapplication.Adapter.TrafficInfoAdapter;
import com.example.myapplication.Adapter.TrafficInfoLineAdapter;
import com.example.myapplication.Item.BusStopItem;
import com.example.myapplication.Item.DataBase;
import com.example.myapplication.Item.LineList;
import com.example.myapplication.Item.TrafficInfoItem;
import com.example.myapplication.R;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashMap;
import java.util.List;
//The activity for display all stations and the alerts of a line
public class ScheduleLineActivity extends AppCompatActivity {
    private BusStopAdapter busStopAdapter;
    private ArrayList<TrafficInfoItem> outerList = new ArrayList<>();
    private TrafficInfoLineAdapter trafficInfoLineAdapter;
    private ArrayList<BusStopItem> busStopItemList = new ArrayList<>();
    Boolean sens = true;
    private String nbLine;
    private LineList lineList;
    @Override
    protected void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.schedule_line_activity);
        //retrieve the LineList from the intent
        lineList = getIntent().getParcelableExtra("lineList");
        TextView t1 = findViewById(R.id.busStopFirst);
        t1.setText("from: "+lineList.getDepart());
        TextView t2 = findViewById(R.id.busStopSecond);
        t2.setText("to: "+lineList.getArrivee());
        ImageView imageView = findViewById(R.id.nbLine);
        imageView.setImageResource(lineList.getImage());
        nbLine = lineList.getNbLine();
        //change the color of the bus stop tab
        changeTitlecolor("busStop");
        ListView listView = findViewById(R.id.listSchedulesLine);
        //retrieve the alerts from the data base singleton
        outerList = (ArrayList<TrafficInfoItem>) DataBase.getInstance().getAlerts();
        createList();
        busStopAdapter = new BusStopAdapter(this,busStopItemList);
        listView.setAdapter(busStopAdapter);
        findViewById(R.id.homeButton).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Intent intent = new Intent(ScheduleLineActivity.this, MainActivity.class);
                startActivity(intent);

            }
        });
        findViewById(R.id.trafficInfoButton).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Intent intent = new Intent(ScheduleLineActivity.this, TrafficInfoActivity.class);
                startActivity(intent);

            }
        });
        findViewById(R.id.retourScheduleLine).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Intent intent = new Intent(ScheduleLineActivity.this, SchedulesActivity.class);
                startActivity(intent);

            }
        });
        findViewById(R.id.switchStationBTN).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {

                TextView t1 = findViewById(R.id.busStopFirst);
                String busStop1 = t1.getText().toString().split(":")[1];

                TextView t2 = findViewById(R.id.busStopSecond);
                String busStop2 = t2.getText().toString().split(":")[1];

                t1.setText("from:"+busStop2);
                t2.setText("to:"+busStop1);
                sens = !sens;
                createList();
                busStopAdapter = new BusStopAdapter(ScheduleLineActivity.this,busStopItemList);
                listView.setAdapter(busStopAdapter);
            }
        });
        findViewById(R.id.busStopBTN).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                changeTitlecolor("busStop");
                listView.setAdapter(busStopAdapter);
            }
        });

        findViewById(R.id.trafficInfoBTN).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                changeTitlecolor("trafficInfo");
                ArrayList<TrafficInfoItem> tmp = new ArrayList<>();
                for (TrafficInfoItem t : outerList){

                    if(t.getNumbers().contains(nbLine)){
                        tmp.add(t);
                    }
                }

                trafficInfoLineAdapter = new TrafficInfoLineAdapter(ScheduleLineActivity.this,tmp);
                listView.setAdapter(trafficInfoLineAdapter);

            }
        });


    }
    private void changeTitlecolor(String title) {
        if(title =="busStop"){
            TextView textTitle = findViewById(R.id.busStopBTN);
            TextView text1 = findViewById(R.id.trafficInfoBTN);
            text1.setTextColor(Color.parseColor("#FFFFFF"));
            textTitle.setTextColor(Color.parseColor("#26474E"));

        }
        else{
            TextView textTitle = findViewById(R.id.trafficInfoBTN);
            TextView text1 = findViewById(R.id.busStopBTN);
            text1.setTextColor(Color.parseColor("#FFFFFF"));
            textTitle.setTextColor(Color.parseColor("#26474E"));

        }
    }
    //create the of the stops of the line
    private void createList(){
        List<BusStopItem> tmp = new ArrayList<>();
        //retrieve the bus stops with the data base singleton
        //create a copy because the list will change
        tmp.addAll(DataBase.getInstance().getStations());
        //check the direction
        //and remove stops that do not have schedules for this direction
        if(sens) {
            for (BusStopItem stop : DataBase.getInstance().getStations()) {
                if ((stop.getSchedulesAller().get(nbLine) == null && stop.getSchedulesRetour().get(nbLine) == null) ||
                        (stop.getSchedulesAller().get(nbLine) == null
                        || stop.getSchedulesAller().get(nbLine).isEmpty())) {
                    tmp.remove(stop);
                }

            }
        }else{
            for (BusStopItem stop : DataBase.getInstance().getStations()) {
                if ((stop.getSchedulesAller().get(nbLine) == null
                        && stop.getSchedulesRetour().get(nbLine) == null)
                        || (stop.getSchedulesRetour().get(nbLine) == null
                        || stop.getSchedulesRetour().get(nbLine).isEmpty())) {
                    tmp.remove(stop);
                }

            }
        }
        busStopItemList = (ArrayList<BusStopItem>) tmp;


    }
    public boolean getSens(){
        return sens;
    }
    public LineList getLine(){
        return lineList;
    }

}
