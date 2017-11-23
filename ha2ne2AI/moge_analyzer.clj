(ns moge-ai.core
  (:gen-class)
  (:import (java.io PrintStream ByteArrayOutputStream
                    InputStreamReader BufferedReader
                    OutputStreamWriter BufferedWriter
                    FileNotFoundException
                    File)
           java.lang.ProcessBuilder))

(def cmd
  ["C:/home/ha2ne2/work/mogeRPGserver132/mogeRPGserver.exe"
   "-d" "0"
   "--no-clear"
   "--ai" "C:/home/ha2ne2/work/ha2ne2AI/ha2ne2AI/bin/Release/ha2ne2AI.exe"])

(defn get-error-reader [process]
  (BufferedReader.
   (InputStreamReader. (.getErrorStream process))))

(defn get-reader [process]
  (BufferedReader.
   (InputStreamReader. (.getInputStream process))))

(defn endless-read [reader]
  (future
    (loop [[line & rest] (line-seq reader)]
      (when line
        (recur rest)))))

(defn s-to-i [s]
  (try
    (Integer/parseInt s)
    (catch java.lang.NumberFormatException e nil)))

(defn format-result [xs] ; -> string
  (clojure.pprint/cl-format
   nil
   "MIN:~A, MAX:~A, CENTER:~A, AVE:~A"
   (reduce min xs)
   (reduce max xs)
   ((vec (sort xs)) (/ (count xs) 2))
   (/ (reduce + xs) (+ (count xs) 0.0))))

(defn analyze [times]
  (loop [i 0 acc []]
    (if (< i times)
      (let [proc (.start (ProcessBuilder. cmd))
            reader (get-reader proc)
            error-reader (get-error-reader proc)]
        (endless-read (get-error-reader proc))
        (recur (inc i)
               (conj acc
                     (loop [[line & rest] (line-seq reader) result nil]
                       (if line
                         (if (clojure.string/starts-with? line "‚ ‚È‚½‚Í")
                           (do
                             (println i ": " line)
                             (recur rest
                                      (s-to-i (second (re-find #"(\d+)" line)))))
                           (recur rest result))
                         result)))))
      (do (println acc)
          (println (format-result acc))
          nil))))


(analyze 100)



