package com.example.myapplication.Item;

import static android.content.Context.MODE_PRIVATE;

import android.content.Context;
import android.os.Environment;
import android.os.Parcel;
import android.os.Parcelable;

import androidx.annotation.NonNull;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.FileWriter;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.OutputStreamWriter;
//This is the class for the Line we will retrieve from the API
public class LineList implements Parcelable {

    private int image;
    private String nbLine;
    //name of the first stop
    private String depart;
    //name of the last stop
    private String arrivee;
    private boolean favorite = false;

    public LineList(int image,String nbLine, String depart, String arrivee) {
        this.image = image;
        this.nbLine = nbLine;
        this.depart = depart;
        this.arrivee = arrivee;
    }

    //Parcelable constructor
    protected LineList(Parcel in) {
        image = in.readInt();
        nbLine = in.readString();
        depart = in.readString();
        arrivee = in.readString();
        favorite = in.readByte() != 0;
    }

    public static final Creator<LineList> CREATOR = new Creator<LineList>() {
        @Override
        public LineList createFromParcel(Parcel in) {
            return new LineList(in);
        }

        @Override
        public LineList[] newArray(int size) {
            return new LineList[size];
        }
    };

    public int getImage() {
        return image;
    }

    public void setImage(int image) {
        this.image = image;
    }

    public String getDepart() {
        return depart;
    }

    public void setDepart(String depart) {
        this.depart = depart;
    }

    public String getArrivee() {
        return arrivee;
    }

    public void setArrivee(String arrivee) {
        this.arrivee = arrivee;
    }

    public boolean isFavorite() {
        return favorite;
    }
    //Set favorite for the line but it write it in a file to be able to recover the
    //favorite's line when the app restart
    public void setFavorite(boolean favorite) {
        this.favorite = favorite;
        if(favorite){
            try {
                //The file is in the phone so you can't retrieve it easily
                File directory = Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DOCUMENTS);

                // Creation of the path to the file
                File file = new File(directory, "favorite.txt");

                FileOutputStream fos = new FileOutputStream(file, true); // 'true' for add text to the file


                OutputStreamWriter writer = new OutputStreamWriter(fos);

                // data to write
                writer.write("|"+ nbLine + "|");
                writer.write("\n");



                writer.close();
                fos.close();


            } catch (IOException e) {
                e.printStackTrace();
            }
        }else{ try {
           //delete the nbLine if it's not a favorite anymore
            File directory = Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DOCUMENTS);

            // Creation of the path to the file
            File file = new File(directory, "favorite.txt");
            FileInputStream fis = new FileInputStream(file);
            InputStreamReader reader = new InputStreamReader(fis);
            BufferedReader bufferedReader = new BufferedReader(reader);

            StringBuilder sb = new StringBuilder();
            String line;
            //read the file for find the good line
            while ((line = bufferedReader.readLine()) != null) {
                //delete the line if it's found
                if (!line.contains("|" + nbLine + "|")) {
                    sb.append(line);
                    sb.append("\n");
                }
            }


            bufferedReader.close();
            reader.close();
            fis.close();

            // write the new content in the file
            FileOutputStream fos = new FileOutputStream(file);
            OutputStreamWriter writer = new OutputStreamWriter(fos);
            writer.write(sb.toString());

            writer.close();
            fos.close();


        } catch (IOException e) {
            e.printStackTrace();
        }
    }
    }



    public String getNbLine() {
        return nbLine;
    }

    public void setNbLine(String nbLine) {
        this.nbLine = nbLine;
    }

    @Override
    public int describeContents() {
        return 0;
    }

    @Override
    public void writeToParcel(@NonNull Parcel dest, int flags) {
        dest.writeInt(image);
        dest.writeString(nbLine);
        dest.writeString(depart);
        dest.writeString(arrivee);
        dest.writeByte((byte) (favorite ? 1 : 0));
    }
}
