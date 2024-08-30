package com.example.myapplication.Adapter;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;

import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;

import com.example.myapplication.R;

import java.util.List;
//Adapter for the lines impacted by an alert
public class InnerTIAdapter extends RecyclerView.Adapter<InnerTIAdapter.InnerViewHolder> {

    private List<String> innerList;

    public InnerTIAdapter(List<String> innerList) {
        this.innerList = innerList;
    }

    @NonNull
    @Override
    public InnerViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.innerlist_alert, parent, false);
        return new InnerViewHolder(view);
    }


    @Override
    public void onBindViewHolder(@NonNull InnerViewHolder holder, int position) {
        //get the number of the line and choose the right logo
        switch (innerList.get(position)){
            case "1":
                holder.imageView.setBackgroundResource(R.drawable.line1);
                break;
            case "2":
                holder.imageView.setBackgroundResource(R.drawable.line2);
                break;
            case "3":
                holder.imageView.setBackgroundResource(R.drawable.line3);
                break;
            case "4":
                holder.imageView.setBackgroundResource(R.drawable.line4);
                break;
            case "5":
                holder.imageView.setBackgroundResource(R.drawable.line5);
                break;
            case "6":
                holder.imageView.setBackgroundResource(R.drawable.line6);
                break;
            default:
                holder.imageView.setBackgroundResource(R.drawable.line6);
                break;
        }

    }


    @Override
    public int getItemCount() {
        return innerList.size();
    }

    class InnerViewHolder extends RecyclerView.ViewHolder {
        ImageView imageView;

        public InnerViewHolder(@NonNull View itemView) {
            super(itemView);
            imageView = itemView.findViewById(R.id.alertLogoBus);
        }
    }
}