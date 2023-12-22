use serde_json;
use std::env;

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

fn main() {
    let args: Vec<String> = env::args().collect();

    let command = &args[1];

    if command == "decode" {
        let encoded_value = &args[2];
        let decoded_value = decode_bencoded_value(encoded_value);

        println!("{}", serde_json::to_string_pretty(&decoded_value).unwrap())
    }
}
