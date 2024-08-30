    package com.example.myapplication.Adapter;

    import android.content.Context;
    import android.content.Intent;
    import android.view.LayoutInflater;
    import android.view.View;
    import android.view.ViewGroup;
    import android.widget.ArrayAdapter;
    import android.widget.ImageView;
    import android.widget.TextView;

    import androidx.annotation.NonNull;
    import androidx.annotation.Nullable;

    import com.example.myapplication.Activity.BusStopScheduleActivity;
    import com.example.myapplication.Activity.ScheduleLineActivity;
    import com.example.myapplication.Activity.SchedulesActivity;
    import com.example.myapplication.Activity.TrafficInfoActivity;
    import com.example.myapplication.Item.LineList;
    import com.example.myapplication.R;

    import java.util.ArrayList;
//Adapter for the lines
    public class LineListAdapter extends ArrayAdapter<LineList> {
        private Context mContext;
        public LineListAdapter(@NonNull Context context, ArrayList<LineList> dataArrayList) {

            super(context, R.layout.listview_line,dataArrayList);
            mContext = context;
        }

        @NonNull
        @Override
        public View getView(int position, @Nullable View view, @NonNull ViewGroup parent) {
            LineList lineList = getItem(position);

            if(view == null){
                view = LayoutInflater.from(getContext()).inflate(R.layout.listview_line,parent,false);
            }
            //display the informations
            ImageView listImage = view.findViewById(R.id.nbLine);
            TextView departTxt = view.findViewById(R.id.departLineTxt);
            TextView arriveeTxt = view.findViewById(R.id.arriveeLineTxt);

            listImage.setImageResource(lineList.getImage());
            departTxt.setText("from: " + lineList.getDepart());
            arriveeTxt.setText("to: " + lineList.getArrivee());

            ImageView favImage = view.findViewById(R.id.favoriteBTN);
            if(lineList.isFavorite())
                favImage.setImageResource(R.drawable.favorite_on);
            else
                favImage.setImageResource(R.drawable.favorite_off);
            //on click to mark as favorite a line
            favImage.setOnClickListener(new View.OnClickListener() {
                @Override
                public void onClick(View v) {
                    lineList.setFavorite(!lineList.isFavorite());
                    if(lineList.isFavorite())
                        favImage.setImageResource(R.drawable.favorite_on);
                    else
                        favImage.setImageResource(R.drawable.favorite_off);
                }
            });
            //on click to go on the ScheduleLineActivity
            view.setOnClickListener(new View.OnClickListener() {
                @Override
                public void onClick(View v) {

                    Intent intent = new Intent(mContext, ScheduleLineActivity.class);
                    intent.putExtra("lineList",lineList);
                    mContext.startActivity(intent);
                }
            });

            return view;
        }
    }
