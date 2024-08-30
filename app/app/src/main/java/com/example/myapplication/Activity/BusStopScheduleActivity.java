package com.example.myapplication.Activity;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.ImageView;
import android.widget.ListView;
import android.widget.TextView;

import androidx.annotation.Nullable;
import androidx.appcompat.app.AppCompatActivity;

import com.example.myapplication.Adapter.BusStopScheduleAdapter;
import com.example.myapplication.Item.BusStopItem;
import com.example.myapplication.Item.LineList;
import com.example.myapplication.R;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.TreeMap;

public class BusStopScheduleActivity extends AppCompatActivity {
    //Map of the schedule
    private HashMap<Integer,List<String>> listHoraire = new HashMap<>();
    private BusStopScheduleAdapter busStopScheduleAdapter;
    //boolean to know the direction asked
    private Boolean aller;
    //line of the bus stop
    private LineList line;
    //bus stop clicked
    private BusStopItem busStop;
    //sorted Map of the schedule
    private TreeMap<Integer, List<String>> horairesTries;
    @Override
    protected void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.bus_stop_schedule_layout);
        //retrieve the objects of the intent
         busStop = getIntent().getParcelableExtra("busStop");
        aller = getIntent().getBooleanExtra("aller",true);
        line = getIntent().getParcelableExtra("line");
        TextView busStopNameTextView = findViewById(R.id.busStopFirst);
        ImageView imageView = findViewById(R.id.nbLine);
        //change the logo of the line according to the number
        switch (line.getNbLine()){
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
        busStopNameTextView.setText(busStop.getName());
        //retrieve the schedules according tot he direction choose
        if(aller)
            setHM(busStop.getSchedulesAller());
        else{
            setHM(busStop.getSchedulesRetour());
        }
        //set the adapter to the ListView
        busStopScheduleAdapter = new BusStopScheduleAdapter(this,R.layout.hour_list,horairesTries);
        ListView listView = findViewById(R.id.listSchedulesBusStop);
        listView.setAdapter(busStopScheduleAdapter);
        //on click for return to schedule page
        findViewById(R.id.retourBusStop).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Intent intent = new Intent(BusStopScheduleActivity.this, SchedulesActivity.class);

                startActivity(intent);
            }
        });
        //on click for return to home page
        findViewById(R.id.homeButton).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Intent intent = new Intent(BusStopScheduleActivity.this, MainActivity.class);
                startActivity(intent);

            }
        });
        //on click for return to traffic info page
        findViewById(R.id.trafficInfoButton).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Intent intent = new Intent(BusStopScheduleActivity.this, TrafficInfoActivity.class);
                startActivity(intent);

            }
        });
        //on click to change the direction of the schedule and display the other schedules
        findViewById(R.id.divToBusStop).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                //change the boolean
                //change the 2 TextView to show the direction change
                aller = !aller;
                if(aller) {
                    TextView textView = findViewById(R.id.busStopSecond);
                    textView.setText("to: " + line.getArrivee());
                    setHM(busStop.getSchedulesAller());
                }
                else{
                    TextView textView = findViewById(R.id.busStopSecond);
                    textView.setText("to: " + line.getDepart());
                    setHM(busStop.getSchedulesRetour());
                }
                //set the adapter to the ListView with the new schedules
                busStopScheduleAdapter = new BusStopScheduleAdapter(BusStopScheduleActivity.this,R.layout.hour_list,horairesTries);
                ListView listView = findViewById(R.id.listSchedulesBusStop);
                listView.setAdapter(busStopScheduleAdapter);
            }
        });
    }
    //function to create the HashMap of the schedueles
    private void setHM(HashMap<String,List<String>> schedulesHM) {
        listHoraire = new HashMap<>();
        //for all schedule
        for(List<String> schedules : schedulesHM.values()) {
            for (String s : schedules) {
                //split the string in 2 with "h" as delimiter
                String[] tab = s.split("h");
                //if the HashMap doesn't constains currently the hour
                //create the list and add it to the HashMap
                //else just add the value corresponding to the minutes in the HashMap
                if (!listHoraire.containsKey(Integer.parseInt(tab[0]))) {
                    List<String> tmp = new ArrayList<>();
                    tmp.add(tab[1]);
                    listHoraire.put(Integer.parseInt(tab[0]), tmp);
                } else {
                    listHoraire.get(Integer.parseInt(tab[0])).add(tab[1]);
                }

            }
        }
        //create a TreeMap to sort the HashMap
         horairesTries = new TreeMap<>(listHoraire);

    }
}
