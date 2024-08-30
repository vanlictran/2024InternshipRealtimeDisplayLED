package com.example.myapplication.Activity;

import android.Manifest;
import android.content.Context;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.content.res.Resources;
import android.graphics.Bitmap;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.drawable.Drawable;
import android.location.Location;
import android.net.ConnectivityManager;
import android.net.NetworkInfo;
import android.os.Bundle;
import android.os.Looper;
import android.util.Log;
import android.view.Gravity;
import android.view.LayoutInflater;
import android.view.View;
import android.widget.FrameLayout;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.PopupWindow;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.appcompat.app.AppCompatActivity;
import androidx.core.app.ActivityCompat;
import androidx.core.content.ContextCompat;
import androidx.fragment.app.Fragment;
import androidx.fragment.app.FragmentActivity;

import com.example.myapplication.Item.DataBase;
import com.example.myapplication.R;
import com.google.android.gms.location.FusedLocationProviderClient;
import com.google.android.gms.location.LocationCallback;
import com.google.android.gms.location.LocationRequest;
import com.google.android.gms.location.LocationResult;
import com.google.android.gms.location.LocationServices;
import com.google.android.gms.maps.CameraUpdate;
import com.google.android.gms.maps.CameraUpdateFactory;
import com.google.android.gms.maps.GoogleMap;
import com.google.android.gms.maps.MapView;
import com.google.android.gms.maps.OnMapReadyCallback;
import com.google.android.gms.maps.SupportMapFragment;
import com.google.android.gms.maps.model.BitmapDescriptor;
import com.google.android.gms.maps.model.BitmapDescriptorFactory;
import com.google.android.gms.maps.model.LatLng;
import com.google.android.gms.maps.model.MapStyleOptions;
import com.google.android.gms.maps.model.MarkerOptions;
import com.google.android.gms.tasks.OnSuccessListener;

import org.json.JSONArray;
import org.json.JSONObject;

import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.net.HttpURLConnection;
import java.net.URL;
//The main activity where the map is displayed and the pop up option
public class MainActivity extends FragmentActivity implements OnMapReadyCallback {
    private boolean english = true;
    FrameLayout mapView;
    GoogleMap gMap;
    LatLng userLocation;
    private FusedLocationProviderClient fusedLocationClient;
    private LocationCallback locationCallback;
    private boolean isFirstLocationUpdate = true;
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        //initialisation of the location service for the user location
        fusedLocationClient = LocationServices.getFusedLocationProviderClient(this);
        //call the function for retrieve the data from the API and check if it's already done or not
        if(!DataBase.getInstance().isDataRetrieve()) {
            DataBase.getInstance().retrieveDataFromAPI();
        }


        ImageView homeImg = findViewById(R.id.homeButton);
        homeImg.setImageResource(R.drawable.home_on);
        TextView hometxt = findViewById(R.id.textHomebutton);
        hometxt.setTextColor(Color.parseColor("#FFC656"));

        mapView = findViewById(R.id.map);
        //creation of the map
        SupportMapFragment mapFragment = (SupportMapFragment) getSupportFragmentManager().findFragmentById(R.id.map);
        mapFragment.getMapAsync(this);

        fusedLocationClient = LocationServices.getFusedLocationProviderClient(this);
        //retrieve the user location in real time
        locationCallback = new LocationCallback() {
            @Override
            public void onLocationResult(LocationResult locationResult) {
                //if no location information quit the function
                if (locationResult == null) {
                    return;
                }
                for (Location location : locationResult.getLocations()) {
                    //create the location and move the map on it
                    userLocation = new LatLng(location.getLatitude(), location.getLongitude());
                    gMap.moveCamera(CameraUpdateFactory.newLatLngZoom(userLocation, 16));
                    //personalization of the marker
                    MarkerOptions markerOptions = new MarkerOptions()
                            .position(userLocation)
                            .icon(bitmapDescriptorFromVector(MainActivity.this, R.drawable.custom_marker));
                    gMap.addMarker(markerOptions);
                    //retrieve the weather only on the fist location update
                    //we consider that the user will not move far enough for this to have an impact on the weather
                    if (isFirstLocationUpdate) {
                        retrieveWeather();
                        isFirstLocationUpdate = false;
                    }
                }
            }
        };
        //on click for show the pop up option menu
        findViewById(R.id.settingsPopup).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                showPopupInitial(v);
            }
        });
        //on click for return to schedule page
        findViewById(R.id.schedulesButton).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Intent intent = new Intent(MainActivity.this, SchedulesActivity.class);
                startActivity(intent);

            }
        });
        //on click for return to taffic info page
        findViewById(R.id.trafficInfoButton).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Intent intent = new Intent(MainActivity.this, TrafficInfoActivity.class);
                startActivity(intent);

            }
        });
        //on click for return to search road page
        findViewById(R.id.searchRoadBTN).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Intent intent = new Intent(MainActivity.this, SearchRoadActivity.class);
                startActivity(intent);

            }
        });
    }
    //function to show the pop up option menu
    private void showPopupInitial(View anchorView) {
        LayoutInflater inflater = (LayoutInflater) getSystemService(LAYOUT_INFLATER_SERVICE);
        View popupView = inflater.inflate(R.layout.popup_parametres, null);


        int width = 700;
        int height = LinearLayout.LayoutParams.WRAP_CONTENT;
        boolean focusable = true;
        final PopupWindow popupWindow = new PopupWindow(popupView, width, height, focusable);
        //active the background according to the language chose
        if (english) {
            ImageView img = popupView.findViewById(R.id.flag_uk);
            img.setBackgroundResource(R.drawable.bordureverte);

        } else {
            ImageView img = popupView.findViewById(R.id.flag_vn);
            img.setBackgroundResource(R.drawable.bordureverte);

        }
        //set the pop up at the center of the screen
        popupWindow.showAtLocation(anchorView.getRootView(), Gravity.CENTER, 0, 0);
        //on click to quit the pop up menu
        popupView.findViewById(R.id.exitParameters).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                // Handle profile click
                popupWindow.dismiss();
            }
        });

        // on click for choose the english language
        popupView.findViewById(R.id.flag_uk).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {

                ImageView img = popupView.findViewById(R.id.flag_uk);
                img.setBackgroundResource(R.drawable.bordureverte);
                ImageView img2 = popupView.findViewById(R.id.flag_vn);
                img2.setBackgroundResource(R.drawable.nobordure);
                english = true;
                //TODO change language
            }
        });
        // on click for choose the vietnamese language
        popupView.findViewById(R.id.flag_vn).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                // Handle flag VN click
                ImageView img = popupView.findViewById(R.id.flag_uk);
                img.setBackgroundResource(R.drawable.nobordure);
                ImageView img2 = popupView.findViewById(R.id.flag_vn);
                img2.setBackgroundResource(R.drawable.bordureverte);
                english = false;
                //TODO change language
            }
        });
        //on click to go to the setting page
        popupView.findViewById(R.id.settingsBTN).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                // Handle settings click
                popupWindow.dismiss();
                showPopupSettings(v);
            }
        });
        //on click to go to the profile page
        popupView.findViewById(R.id.profileBTN).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                // Handle profile click
                popupWindow.dismiss();
                showPopupProfile(v);
            }
        });
        //on click to go to the help page
        popupView.findViewById(R.id.helpBTN).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                //TODO help page
            }
        });
        //on click to quit the application
        popupView.findViewById(R.id.exitBTN).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {

                finishAffinity();
                System.exit(0);
            }
        });
    }
    //function to show the setting page
    private void showPopupSettings(View anchorView) {
        LayoutInflater inflater = (LayoutInflater) getSystemService(LAYOUT_INFLATER_SERVICE);
        View popupView = inflater.inflate(R.layout.popup_settings, null);


        int width = 700;
        int height = LinearLayout.LayoutParams.WRAP_CONTENT;
        boolean focusable = true;
        final PopupWindow popupWindow = new PopupWindow(popupView, width, height, focusable);

        //set the pop up at the center of the screen
        popupWindow.showAtLocation(anchorView.getRootView(), Gravity.CENTER, 0, 0);
        //on click to return to the inital pop up menu
        popupView.findViewById(R.id.exitParameters).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                // Handle profile click
                popupWindow.dismiss();
                showPopupInitial(v);
            }
        });

        //on click to interact with the notification button
        popupView.findViewById(R.id.notifBTN).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                //TODO notification settings
            }
        });
        //on click to interact with the sound button
        popupView.findViewById(R.id.soundsBTN).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                //TODO sound settings
            }
        });


    }
    //function to show the profile page
    private void showPopupProfile(View anchorView) {
        LayoutInflater inflater = (LayoutInflater) getSystemService(LAYOUT_INFLATER_SERVICE);
        View popupView = inflater.inflate(R.layout.popup_profile, null);


        int width = 700;
        int height = LinearLayout.LayoutParams.WRAP_CONTENT;
        boolean focusable = true;
        final PopupWindow popupWindow = new PopupWindow(popupView, width, height, focusable);

        //set the pop up at the center of the screen
        popupWindow.showAtLocation(anchorView.getRootView(), Gravity.CENTER, 0, 0);
        //on click to return to the inital pop up menu
        popupView.findViewById(R.id.exitParameters).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                // Handle profile click
                popupWindow.dismiss();
                showPopupInitial(v);
            }
        });

        //on click to connect to an account
        popupView.findViewById(R.id.connectBTN).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                // Handle profile click
                popupWindow.dismiss();
                //TODO connexion
            }
        });
            }

    //function for convert the drawable into an icon
    private BitmapDescriptor bitmapDescriptorFromVector(Context context, int vectorResId) {
        Drawable vectorDrawable = ContextCompat.getDrawable(context, vectorResId);
        vectorDrawable.setBounds(0, 0, vectorDrawable.getIntrinsicWidth(), vectorDrawable.getIntrinsicHeight());
        Bitmap bitmap = Bitmap.createBitmap(vectorDrawable.getIntrinsicWidth(), vectorDrawable.getIntrinsicHeight(), Bitmap.Config.ARGB_8888);
        Canvas canvas = new Canvas(bitmap);
        vectorDrawable.draw(canvas);
        return BitmapDescriptorFactory.fromBitmap(bitmap);
    }
    //function for update the location
    private void startLocationUpdates() {
        LocationRequest locationRequest = LocationRequest.create();
        //set the intervals and priority
        locationRequest.setInterval(10000);
        locationRequest.setFastestInterval(5000);
        locationRequest.setPriority(LocationRequest.PRIORITY_HIGH_ACCURACY);
        //check the permission
        if (ActivityCompat.checkSelfPermission(this, Manifest.permission.ACCESS_FINE_LOCATION)
                != PackageManager.PERMISSION_GRANTED && ActivityCompat.checkSelfPermission(this, Manifest.permission.ACCESS_COARSE_LOCATION)
                != PackageManager.PERMISSION_GRANTED) {
            return;
        }
        //call the location update on fusedLocationClient in real time
        fusedLocationClient.requestLocationUpdates(locationRequest, locationCallback, Looper.getMainLooper());
    }
    //at the start of the activity call the location update
    @Override
    protected void onStart() {
        super.onStart();
        if (ContextCompat.checkSelfPermission(this, Manifest.permission.ACCESS_FINE_LOCATION)
                == PackageManager.PERMISSION_GRANTED) {
            startLocationUpdates();
        }
    }
    //at the end of the activity stop the location update
    @Override
    protected void onStop() {
        super.onStop();
        fusedLocationClient.removeLocationUpdates(locationCallback);
    }
    //creation of the map when it's ready
    @Override
    public void onMapReady(@NonNull GoogleMap googleMap) {
        this.gMap = googleMap;

        googleMap.setMapType(GoogleMap.MAP_TYPE_NORMAL);
        //check permission or request it
        if (ContextCompat.checkSelfPermission(this, Manifest.permission.ACCESS_FINE_LOCATION)
                == PackageManager.PERMISSION_GRANTED) {
            gMap.setMyLocationEnabled(true);
            startLocationUpdates();
        } else {
            ActivityCompat.requestPermissions(this, new String[]{Manifest.permission.ACCESS_FINE_LOCATION}, 1);
        }
    }


    //fucntion to retrieve weather data from an API
    private void retrieveWeather() {
        new Thread(new Runnable() {
            @Override
            public void run() {
                //set variables and url
                String lat = ""+userLocation.latitude;
                String lon = ""+userLocation.longitude;
                String apiKey = "d8cf44c10cf0a4aa3d330939e8562642";
                String apiUrl = "https://api.openweathermap.org/data/2.5/weather?lat="+lat+"&lon="+lon+"&appid="+apiKey;
               try {
                    URL url = new URL(apiUrl);
                    HttpURLConnection conn = (HttpURLConnection) url.openConnection();
                    conn.setRequestMethod("GET");

                    BufferedReader in = new BufferedReader(new InputStreamReader(conn.getInputStream()));
                    String inputLine;
                    StringBuilder response = new StringBuilder();
                    //retrieve the answer
                    while ((inputLine = in.readLine()) != null) {
                        response.append(inputLine);
                    }
                    in.close();

                    //Create a JSON object
                    JSONObject jsonResponse = new JSONObject(response.toString());
                    //get the weather description
                    JSONArray weatherArray = jsonResponse.getJSONArray("weather");
                    JSONObject weatherObject = weatherArray.getJSONObject(0);

                    String weatherMain = weatherObject.getString("main").toLowerCase();
                    //get the temperature
                    JSONObject mainObject = jsonResponse.getJSONObject("main");
                    int temp = (int) (mainObject.getDouble("temp") - 273.15);

                    //change the logo of the weather according to the description given by the API
                    ImageView logoWeather = findViewById(R.id.circleIcon);
                    if(weatherMain.contains("clouds")){
                        logoWeather.setImageResource(R.drawable.cloudy);
                    }else if(weatherMain.contains("clear")){
                        logoWeather.setImageResource(R.drawable.sunny);
                    }else if(weatherMain.contains("snow")){
                        logoWeather.setImageResource(R.drawable.snow);
                    }else if(weatherMain.contains("thunder")){
                        logoWeather.setImageResource(R.drawable.thunder);
                    }else if(weatherMain.contains("mist")){
                        logoWeather.setImageResource(R.drawable.mist);
                    }else {
                        logoWeather.setImageResource(R.drawable.rainy);
                    }
                    //display the temperature
                    TextView tempTxt = findViewById(R.id.temperature);
                    tempTxt.setText(temp +" °C");


                } catch (Exception e) {
                    e.printStackTrace();
                }
            }
        }).start();
    }

}


