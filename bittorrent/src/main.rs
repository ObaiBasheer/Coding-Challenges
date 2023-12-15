use serde_json;
use std::env;

fn decode_bencoded_value(encoded_value: &str) -> serde_json::Value {
    //if encoded_Value strats with  adigit it's a number
    match &encoded_value[0] {
        'i' => {
            if let Some(rest) = encoded_value.strip_prefix('i') {
                if let Some((digits, _)) = rest.split_once('e') {
                    if let Ok(n) = digits.parse::<i64>() {
                        return n.into();
                    }
                }
            }
        }
        'l' => {}
        '0'..='9' => {
            if let Some((len, rest)) = encoded_value.split_once(':') {
                if let Ok(len) = len.parse::<usize>() {
                    return serde_json::Value::String(rest[..len].to_string());
                }
            }
        }
    }

    panic!("Unhandled encoded value : {}", encoded_value);
}

fn main() {
    let args: Vec<String> = env::args().collect();

    let command = &args[1];

    if command == "decode" {
        let encoded_value = &args[2];
        let decoded_value = decode_bencoded_value(encoded_value);

        println!("{}", serde_json::to_string_pretty(&decoded_value).unwrap())
    }
}
