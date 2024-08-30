package com.example.myapplication.Adapter;

import android.util.TypedValue;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.example.myapplication.R;
import com.example.myapplication.Item.TrafficInfoItem;

import java.util.List;
//Adapter for the alerts in a RecyclerView
public class TrafficInfoAdapter extends RecyclerView.Adapter<TrafficInfoAdapter.OuterViewHolder> {

    private List<TrafficInfoItem> outerList;

    public TrafficInfoAdapter(List<TrafficInfoItem> outerList) {
        this.outerList = outerList;
    }

    @NonNull
    @Override
    public OuterViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view;
        view = LayoutInflater.from(parent.getContext()).inflate(R.layout.listview_alert, parent, false);
        return new OuterViewHolder(view);
    }



    @Override
    public void onBindViewHolder(@NonNull OuterViewHolder holder, int position) {
        //display informations
        TrafficInfoItem item = outerList.get(position);
        holder.alertTitle.setText(item.getTitle());
        holder.description.setText(item.getDescription());
        // Call the adapter for the list of the lines
        InnerTIAdapter innerAdapter = new InnerTIAdapter(item.getNumbers());
        holder.innerRecyclerView.setAdapter(innerAdapter);
        //on click to show/hide the description of the alert
        holder.plusBtn.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                if(holder.description.getTextSize()== 0){
                    holder.description.setTextSize(TypedValue.COMPLEX_UNIT_DIP, 24);
                    holder.plusBtn.setRotation(90);
                    ViewGroup.LayoutParams layoutParams = holder.plusBtn.getLayoutParams();
                    layoutParams.width = (int) TypedValue.applyDimension(TypedValue.COMPLEX_UNIT_DIP, 48, v.getResources().getDisplayMetrics());
                    layoutParams.height = (int) TypedValue.applyDimension(TypedValue.COMPLEX_UNIT_DIP, 48, v.getResources().getDisplayMetrics());
                    holder.plusBtn.setLayoutParams(layoutParams);
                }
                else{
                    holder.description.setTextSize(TypedValue.COMPLEX_UNIT_DIP, 0);
                    holder.plusBtn.setRotation(0);
                }
                 }
        });
        // Set the LinearLayoutManager with a horizontal orientation for the internal RecyclerView
        holder.innerRecyclerView.setLayoutManager(new LinearLayoutManager(holder.itemView.getContext(), LinearLayoutManager.HORIZONTAL, false));
    }

    @Override
    public int getItemCount() {
        return outerList.size();
    }

    class OuterViewHolder extends RecyclerView.ViewHolder {
        TextView alertTitle;
        ImageView plusBtn;
        TextView description;
        RecyclerView innerRecyclerView;

        public OuterViewHolder(@NonNull View itemView) {
            super(itemView);
            alertTitle = itemView.findViewById(R.id.alert_title);
            description = itemView.findViewById(R.id.descriptionAlert);
            plusBtn = itemView.findViewById(R.id.moreInfo);
            innerRecyclerView = itemView.findViewById(R.id.affectedLinesList);
        }
    }
}

