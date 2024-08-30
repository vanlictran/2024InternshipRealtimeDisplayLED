package com.example.myapplication.Activity;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;

import androidx.appcompat.app.AppCompatActivity;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.example.myapplication.Adapter.TrafficInfoAdapter;
import com.example.myapplication.Item.DataBase;
import com.example.myapplication.R;
import com.example.myapplication.Item.TrafficInfoItem;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

//The activity for display all the alerts
public class TrafficInfoActivity extends AppCompatActivity {
private List<TrafficInfoItem> outerList = new ArrayList<>();
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.trafficinfo_layout);
        //retrieve the RecyclerView with his id
        RecyclerView outerRecyclerView = findViewById(R.id.listTrafficInfo);
        outerRecyclerView.setLayoutManager(new LinearLayoutManager(this, LinearLayoutManager.VERTICAL, false));
        //retrieve the alerts from the data base singleton
        outerList = DataBase.getInstance().getAlerts();
        //set the adapter to the RecyclerView
        TrafficInfoAdapter outerAdapter = new TrafficInfoAdapter(outerList);
        outerRecyclerView.setAdapter(outerAdapter);
        //on click for return to home page
        findViewById(R.id.homeButton).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Intent intent = new Intent(TrafficInfoActivity.this, MainActivity.class);
                startActivity(intent);

            }
        });
        //on click for return to schedule page
        findViewById(R.id.schedulesButton).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Intent intent = new Intent(TrafficInfoActivity.this, SchedulesActivity.class);

                startActivity(intent);

            }
        });



    }

}
