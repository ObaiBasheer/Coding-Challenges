use serde_json;
use serde::Deserialize;
use std::env;

#[derive(Parser, Debug)]
#[command(author, version, about, long_about = None)]
struct Args {
    #[command(subcommand)]
    command: Command,
}
/// A Metainfo files (also known as .torrent files):
#[derive!(Debug, Clone,Deserialize)]
struct Torrent {
    ///The URL of the tracker.
    announce : reqwest::Url,

    info:Info
}
struct Info {
    ///the suggested name to save the file (or directory) as.It is purely advisory.
    name: String,

   /// length maps to the number of bytes in each piece the file is split into.
   /// For the purposes of transfer,
   /// files are split into fixed-size pieces which are all the same length except for possibly the last one which may be truncated.
   /// piece length is almost always a power of two, most commonly 2^18 = 256 K (BitTorrent prior to version 3.2 uses 2 20 = 1 M as default).
   #[serde(rename = "piece Length")]
   plength : usize,

    ///maps to a string whose length is a multiple of 20.
    /// It is to be subdivided into strings of length 20,
    /// each of which is the SHA1 hash of the piece at the corresponding index.
    pieces :Vec<[u8; 20]>,

    /// key length or a key files, but not both or neither.
    /// If length is present then the download represents a single file,
    /// otherwise it represents a set of files which go in a directory structure.
    #[serde(faltten)]
    keys: Keys
}

#[derive(Subcommand, Debug)]
enum Command {
    Decode { value: String },
    Info { torrent: PathBuf },
}
#[serde(untagged)]
enum Keys {
    ///In the single file case, length maps to the length of the file in bytes.
singleFile {
    length: usize
},

/// the multi-file case is treated as only having a single file by concatenating the files in the order they appear in the files list.
/// The files list is the value files maps to, and is a list of dictionaries containing the following keys:
/// length and path
MultiFile { files: Vec<File> },
}

struct File {
    /// length - The length of the file, in bytes.
    length: usize,

    /// path - A list of UTF-8 encoded strings corresponding to subdirectory names,
    /// the last of which is the actual file name (a zero length list is an error case).
    path: Vec<String>
}

fn decode_bencoded_value(encoded_value: &str) ->  (serde_json::Value, &str) {
    //if encoded_Value strats with  adigit it's a number
    match encoded_value.chars().next()   {
        Some('i') => {
            if let Some((n, rest)) =
                encoded_value
                    .split_at(1)
                    .1
                    .split_once('e')
                    .and_then(|(digits, rest)| {
                        let n = digits.parse::<i64>().ok()?;
                        Some((n, rest))
                    })
            {
                return (n.into(), rest);
            }
        }
        Some('l') => { //l5:helloi52ee
            let mut values = Vec::new();
            let mut rest = encoded_value.split_at(1).1;
            while !rest.is_empty() && !rest.starts_with('e') {
                let (v, reminder) = decode_bencoded_value(rest);
                values.push(v);
                rest =reminder;
            }
            return (values.into(), &rest[1..])
        }
        Some('d') => { //d3:foo3:bar5:helloi52ee
            let mut dict = serde_json::Map::new();
            let mut rest = encoded_value.split_at(1).1;
            while !rest.is_empty() && !rest.starts_with('e') {
                let (k, reminder) = decode_bencoded_value(rest);
                let k = match k {
                    serde_json::Value::String(k) => k,
                    k =>{
                        panic!("dict keys must be string not {:?}", k);
                    }
                };
                let (v, reminder) = decode_bencoded_value(reminder);
                dict.insert(k,v);
                rest =reminder;
            }
            return (dict.into(), &rest[1..])
        }
        Some('0'..='9') => {
            if let Some((len, rest)) = encoded_value.split_once(':') {
                if let Ok(len) = len.parse::<usize>() {
                    return (rest[..len].to_string().into(),&rest[len..]);
                }
            }
        }
        _ => todo!()
    }

    panic!("Unhandled encoded value : {}", encoded_value);
}

fn main() -> anyhow::Result<()> {
    let args = Args::parse();

    match args.command {
        Command::Decode { value } => {
            // let v: serde_json::Value = serde_bencode::from_str(&value).unwrap();
            // println!("{v}");
            unimplemented!("serde_bencode -> serde_json::Value is borked");
        }
        Command::Info { torrent } => {
            let dot_torrent = std::fs::read(torrent).context("read torrent file")?;
            let t: Torrent =
                serde_bencode::from_bytes(&dot_torrent).context("parse torrent file")?;
            eprintln!("{t:?}");
            println!("Tracker URL: {}", t.announce);
            if let Keys::SingleFile { length } = t.info.keys {
                println!("Length: {length}");
            } else {
                todo!();
            }
        }
    }

    Ok(())
}
mod hashes {
    use serde::de::{self, Deserialize, Deserializer, Visitor};
    use std::fmt;

    #[derive(Debug, Clone)]
    pub struct Hashes(pub Vec<[u8; 20]>);

    struct HashesVisitor;

    impl<'de> Visitor<'de> for HashesVisitor {
        type Value = Hashes;

        fn expecting(&self, formatter: &mut fmt::Formatter) -> fmt::Result {
            formatter.write_str("a byte string whose length is a multiple of 20")
        }

        fn visit_bytes<E>(self, v: &[u8]) -> Result<Self::Value, E>
            where
                E: de::Error,
        {
            if v.len() % 20 != 0 {
                return Err(E::custom(format!("length is {}", v.len())));
            }
            // TODO: use array_chunks when stable
            Ok(Hashes(
                v.chunks_exact(20)
                    .map(|slice_20| slice_20.try_into().expect("guaranteed to be length 20"))
                    .collect(),
            ))
        }
    }

    impl<'de> Deserialize<'de> for Hashes {
        fn deserialize<D>(deserializer: D) -> Result<Self, D::Error>
            where
                D: Deserializer<'de>,
        {
            deserializer.deserialize_bytes(HashesVisitor)
        }
    }
}
