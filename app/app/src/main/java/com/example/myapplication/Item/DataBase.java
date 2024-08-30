package com.example.myapplication.Item;

import android.os.Environment;

import com.example.myapplication.R;

import org.json.JSONArray;
import org.json.JSONObject;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStreamReader;
import java.net.HttpURLConnection;
import java.net.URL;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashMap;
import java.util.List;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
//This is the class where the data will be retrieve from the API
public class DataBase {
    //instance for have a Singleton of this class
    private static DataBase instance;
    //List of the alerts retrieved
    List<TrafficInfoItem> alerts= new ArrayList<>();
    //list of the stops retrieved
    List<BusStopItem> stations = new ArrayList<>();
    //list of the lines retrieved
    List<LineList> lines = new ArrayList<>();

    private ExecutorService executorService = Executors.newSingleThreadExecutor();
    //boolean to know if the data for the main direction of the line is retrieved
    private boolean resAller;
    //boolean to know if the data for the opposite direction of the line is retrieved
    private boolean resRetour;

    public DataBase(){

    }

    public DataBase(List<BusStopItem> stations, List<LineList> lines) {
        this.stations = stations;
        this.lines = lines;
    }
    //Singleton function
    public static  DataBase getInstance() {
        if (instance == null) {
            instance = new DataBase();
        }
        return instance;
    }

    //retrieve all the data from the API
    public boolean retrieveDataFromAPI() {

        executorService.execute(() -> {
            //10.0.2.2 is the address to connect to the localhost of the computer
            //with the phone emulator
            //Will be changed if the API is on a server
            //Schedule/aller is the path to retrieve all the schedules of all the stations
            //in the API for the main direction
            String urlString = "http://10.0.2.2:8000/api/Schedule/aller";

            try {
                //connection to the API
                URL url = new URL(urlString);
                HttpURLConnection conn = (HttpURLConnection) url.openConnection();
                conn.setRequestMethod("GET");
                //UTF-8 because vietnamese language contains lot of accent
                conn.setRequestProperty("Accept-Charset", "UTF-8");

                int responseCode = conn.getResponseCode();

                BufferedReader in = new BufferedReader(new InputStreamReader(conn.getInputStream()));

                String inputLine;
                StringBuilder content = new StringBuilder();
                //retrieve the answer
                while ((inputLine = in.readLine()) != null) {
                    content.append(inputLine);
                }

                in.close();
                conn.disconnect();



                JSONArray jsonArray = new JSONArray(content.toString());

                String firstStop ="";
                String lastStop ="";
                //for each data in the jsonArray we will retrieve each object
                for (int i = 0; i < jsonArray.length(); i++) {
                    JSONObject jsonObject = jsonArray.getJSONObject(i);

                    //the list of schedules named "schedules" ih the Json Array
                    JSONArray schedulesArray = jsonObject.getJSONArray("schedules");
                    //the name of the stop named "name" ih the Json Array
                    String name = jsonObject.getString("name");


                    //each schedule is stored in a arrayList and in a HashMap
                     List<String> schedules = new ArrayList<>();
                    for (int j = 0; j < schedulesArray.length(); j++) {
                        schedules.add(schedulesArray.getString(j));
                    }


                    HashMap<String,List<String>> tmp = new HashMap<>();
                    //Currently the API only contains the line 5 so this is hard coded
                    //But if the API evolves, need to change this to retrieve the good line
                    tmp.put("5",schedules);
                    //The only city cover is Da Nang
                    stations.add(new BusStopItem(name,"Da Nang",tmp));
                    //store the first and last stop to create the LineList
                    if(i==0){
                        firstStop = name;
                    }else if(i== (jsonArray.length()-1)){
                       lastStop = name;
                    }
                }
                //call function to create line
                createLines(firstStop,lastStop);
                //if there is no problem here we consider the data has been retrieve
                resAller = true;
                //call function to retrieve the data of the other direction
                retrieveSchedulesFromReturnAPI();
            } catch (Exception e) {
                e.printStackTrace();
                System.out.println("Erreur lors de la récupération des données : " + e.getMessage());
                resAller = false;
            }
        });
        //call function to create alert
        createAlert();
        return resRetour && resAller;
    }
    //retrieve the data fo the opposite direction from the API
    private boolean retrieveSchedulesFromReturnAPI() {


        executorService.execute(() -> {
            //10.0.2.2 is the address to connect to the localhost of the computer
            //with the phone emulator
            //Will be changed if the API is on a server
            //Schedule/retour is the path to retrieve all the schedules of all the stations
            //in the API for the main direction
            String urlString = "http://10.0.2.2:8000/api/Schedule/retour";
            try {
                URL url = new URL(urlString);
                HttpURLConnection conn = (HttpURLConnection) url.openConnection();
                conn.setRequestMethod("GET");
                conn.setRequestProperty("Accept-Charset", "UTF-8");

                int responseCode = conn.getResponseCode();
                if (responseCode == HttpURLConnection.HTTP_OK) {
                    BufferedReader in = new BufferedReader(new InputStreamReader(conn.getInputStream()));
                    StringBuilder content = new StringBuilder();
                    String inputLine;
                    //retrieve the answer
                    while ((inputLine = in.readLine()) != null) {
                        content.append(inputLine);
                    }

                    in.close();
                    conn.disconnect();

                    JSONArray jsonArray = new JSONArray(content.toString());

                    //for each data in the jsonArray we will retrieve each object
                    for (int i = 0; i < jsonArray.length(); i++) {
                        JSONObject jsonObject = jsonArray.getJSONObject(i);
                        //the list of schedules named "schedules" ih the Json Array

                        JSONArray schedulesArray = jsonObject.getJSONArray("schedules");


                        List<String> schedules = new ArrayList<>();
                        for (int j = 0; j < schedulesArray.length(); j++) {
                            schedules.add(schedulesArray.getString(j));
                        }
                        //the name of the stop named "name" ih the Json Array
                        String name = jsonObject.getString("name");
                        //Currently the API only contains the line 5 so this is hard coded
                        //But if the API evolves, need to change this to retrieve the good line

                        HashMap<String,List<String>> tmp = new HashMap<>();
                        tmp.put("5",schedules);
                        //we create a temporary BusStopItem to check if our list already contains
                        //a stop with the same name
                        //if it contains -> we add the schedules to the item we already have
                        //if it doesn't we create a new BusStopItem
                        BusStopItem tmpB = new BusStopItem(name,"Da Nang");
                        if(stations.contains(tmpB)){
                            for(BusStopItem b : stations){
                                if(b.equals(tmpB)){
                                    b.setSchedulesRetour(tmp);
                                }
                            }
                        }else {
                            stations.add(new BusStopItem(name, "Da Nang", new HashMap<>(), tmp));
                        }

                    }


                } else {
                    System.out.println("Erreur HTTP pour retour API: " + responseCode);
                }
                //if there is no problem here we consider the data has been retrieve

                resRetour= true;

            } catch (Exception e) {
                e.printStackTrace();
                System.out.println("Erreur lors de la récupération des données de retour : " + e.getMessage());
                resRetour = false;
            }
        });
        return resRetour;
    }
    //this function create the list of alert
    //Currently there is no extern service who can bring us this information
    //so for the test a fake list is created
    public void createAlert(){
        alerts.add(new TrafficInfoItem("Alert Title 1","descrition Alert 1", Arrays.asList("1", "2", "3", "4", "5", "6")));
        alerts.add(new TrafficInfoItem("Alert Title 2","descrition Alert 2", Arrays.asList("1")));
        alerts.add(new TrafficInfoItem("Alert Title 3","descrition Alert 3", Arrays.asList("1", "2", "3", "4", "5", "6")));
        alerts.add(new TrafficInfoItem("Alert Title 4","descrition Alert 4", Arrays.asList("1", "5")));

    }
    //Create the LineList and verify if this line is a favorite or not
    public void createLines(String firstStop,String lastStop){
        //Currently the API only contains the line 5 so this is hard coded
        //But if the API evolves, need to change this to retrieve the good line

        LineList tmpLine = new LineList(R.drawable.line5,"5",firstStop,lastStop);
        lines.add(tmpLine);


        //go check the text file for the favorite lines
        File directory = Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DOCUMENTS);
        File file = new File(directory, "favorite.txt");

        // Read the file to check if the line number is present
        try {
            FileInputStream fis = new FileInputStream(file);
            InputStreamReader reader = new InputStreamReader(fis);
            BufferedReader bufferedReader = new BufferedReader(reader);

            String line;
            boolean found = false;
            while ((line = bufferedReader.readLine()) != null) {
                if (line.contains("|"+tmpLine.getNbLine()+"|")) {
                    found = true;
                    break;
                }
            }

            bufferedReader.close();
            reader.close();
            fis.close();
            //if found set the favorite attribute of the lineList at true
            if (found) {
                tmpLine.setFavorite(true);
            }

        } catch (IOException e) {
            e.printStackTrace();
        }
    }
    public List<TrafficInfoItem> getAlerts() {
        return alerts;
    }

    public void setAlerts(List<TrafficInfoItem> alerts) {
        this.alerts = alerts;
    }

    public List<BusStopItem> getStations() {
        return stations;
    }

    public void setStations(List<BusStopItem> stations) {
        this.stations = stations;
    }

    public List<LineList> getLines() {
        return lines;
    }

    public void setLines(List<LineList> lines) {
        this.lines = lines;
    }

    public boolean isDataRetrieve(){
        return resAller && resRetour;
    }


}
