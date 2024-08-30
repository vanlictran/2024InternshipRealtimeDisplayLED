package com.example.myapplication.Activity;

import android.content.Intent;
import android.graphics.Color;
import android.os.Bundle;
import android.view.View;
import android.widget.ListView;
import android.widget.TextView;

import androidx.annotation.Nullable;
import androidx.appcompat.app.AppCompatActivity;

import com.example.myapplication.Adapter.BusStopAdapter;
import com.example.myapplication.Adapter.BusStopGeneralAdapter;
import com.example.myapplication.Adapter.LineListAdapter;
import com.example.myapplication.Item.BusStopItem;
import com.example.myapplication.Item.DataBase;
import com.example.myapplication.Item.LineList;
import com.example.myapplication.R;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

//the activity for display the lines and the bus stations
public class SchedulesActivity extends AppCompatActivity {

    //the 3 lists we will display in this activity
    private ArrayList<LineList> lineLists = new ArrayList<>();
    private ArrayList<LineList> lineListsFavorite = new ArrayList<>();
    private ArrayList<BusStopItem> busStopItemList = new ArrayList<>();
    LineListAdapter lineListAdapter ;
    BusStopGeneralAdapter busStopAdapter;

    @Override
    protected void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.schedules_layout);
        //change the color of the active tab
        changeTitlecolor("line");
        //retrieve the bus lines from the data base singleton
        lineLists = (ArrayList<LineList>) DataBase.getInstance().getLines();
        //set the adapter to the listView
        lineListAdapter = new LineListAdapter(SchedulesActivity.this,lineLists);
        ListView listView = findViewById(R.id.listSchedules);
        listView.setAdapter(lineListAdapter);

        //on click for return to home page
        findViewById(R.id.homeButton).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Intent intent = new Intent(SchedulesActivity.this, MainActivity.class);
                startActivity(intent);

            }
        });
        //on click for return to taffic info page
        findViewById(R.id.trafficInfoButton).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Intent intent = new Intent(SchedulesActivity.this, TrafficInfoActivity.class);
                startActivity(intent);

            }
        });
        //on click for go to favorite list tab
        findViewById(R.id.titleFavoriteTxt).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                changeTitlecolor("favorite");
                //check in the list of all the line which line is check as a favorite
                lineListsFavorite = new ArrayList<>();
                for(LineList l : lineLists){
                    if(l.isFavorite()){
                        lineListsFavorite.add(l);
                    }
                }
                //set the adapter to the listView with the favorite lines
                lineListAdapter = new LineListAdapter(SchedulesActivity.this,lineListsFavorite);
                listView.setAdapter(lineListAdapter);

            }
        });
        //on click for go to the line list tab
        findViewById(R.id.titleLineTxt).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                changeTitlecolor("line");
                //set the adapter to the listView with all the lines
                lineListAdapter = new LineListAdapter(SchedulesActivity.this,lineLists);
                listView.setAdapter(lineListAdapter);

            }
        });
        //on click for go to the bus stop list tab
        findViewById(R.id.titleBusstopTxt).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                changeTitlecolor("busstop");
                //retrieve all the bus stop from the data bas singleton
                busStopItemList = (ArrayList<BusStopItem>) DataBase.getInstance().getStations();
                //set the adapter to the listView all the bus stops
                busStopAdapter = new BusStopGeneralAdapter(SchedulesActivity.this,busStopItemList);
                listView.setAdapter(busStopAdapter);

            }
        });
        listView.setClickable(true);
    }
    //function to change the color of the active tab
    private void changeTitlecolor(String title) {
        //for each tab change his color at black/grey and the other to white
        if(title =="line"){
            TextView textTitle = findViewById(R.id.titleLineTxt);
            TextView text1 = findViewById(R.id.titleFavoriteTxt);
            TextView text2 = findViewById(R.id.titleBusstopTxt);
                text1.setTextColor(Color.parseColor("#FFFFFF"));
            text2.setTextColor(Color.parseColor("#FFFFFF"));
            textTitle.setTextColor(Color.parseColor("#26474E"));

        }
        else if(title =="favorite"){
            TextView textTitle = findViewById(R.id.titleFavoriteTxt);
            TextView text1 = findViewById(R.id.titleLineTxt);
            TextView text2 = findViewById(R.id.titleBusstopTxt);
            text1.setTextColor(Color.parseColor("#FFFFFF"));
            text2.setTextColor(Color.parseColor("#FFFFFF"));
            textTitle.setTextColor(Color.parseColor("#26474E"));

        }
        else{
            TextView textTitle = findViewById(R.id.titleBusstopTxt);
            TextView text1 = findViewById(R.id.titleFavoriteTxt);
            TextView text2 = findViewById(R.id.titleLineTxt);
            text1.setTextColor(Color.parseColor("#FFFFFF"));
            text2.setTextColor(Color.parseColor("#FFFFFF"));
            textTitle.setTextColor(Color.parseColor("#26474E"));

        }
    }

}
