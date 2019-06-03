<?php

	class CrayonLibWrapper_Json {
		public static function lib_json_parse($vm, $args) {
			$raw = $args->arr[0]->internalValue;
			if ((strlen($raw) > 0)) {
				$output = (new LibJsonHelper($vm->globals, $raw))->parse();
				if (($output != null)) {
					return $output;
				}
			}
			return $vm->globalNull;
		}
	}

	// Why not json_decode?
	// - PHP built in JSON decoding does not distinguish {} and []
	// - The data types need to be converted into VM types so some sort of recursion is needed anyway.
	class LibJsonHelper {
		
		var $text;
		var $globals;
		var $index = 0;
		var $length;
		var $error_at = null;
		var $has_error = false;
		var $unicode_by_hex = array();
		
		function __construct($globals, $text) {
			$this->text = $text;
			$this->length = strlen($text);
			$this->globals = $globals;
		}
		
		public function parse() {
			$value = $this->parse_thing();
			if ($value !== null) {
				$this->skip_whitespace();
				if ($this->index != $this->length) {
					return $this->error();
				}
			}
			return $value;
		}
		
		private function parse_thing() {
			$this->skip_whitespace();
			
			if ($this->index < $this->length) {
				switch ($this->text[$this->index]) {
					case "{": return $this->parse_object();
					case "[": return $this->parse_list();
					case "t":
						if ($this->is_next('true')) return $this->globals->boolTrue;
						return $this->error();
					case "f":
						if ($this->is_next('false')) return $this->globals->boolFalse;
						return $this->error();
					case "n":
						if ($this->is_next('null')) return $this->globals->valueNull;
						return $this->error();
					case "\"": return $this->parse_string();
					default: return $this->parse_number();
				}
			}
		}
		
		private function parse_number() {
			$num = array();
			$dec = array();
			$dec_found = false;
			$n0 = ord('0');
			$n9 = ord('9');
			$terminated = false;
			for ($i = $this->index; $i < $this->length; ++$i) {
				$c = $this->text[$i];
				if ($c == '.') {
					if ($dec_found) {
						return $this->error();
					}
					$dec_found = true;
				} else if (ord($c) >= $n0 && ord($c) <= $n9) {
					if ($dec_found) {
						array_push($dec, $c);
					} else {
						array_push($num, $c);
					}
				} else {
					$this->index = $i;
					$terminated = true;
					break;
				}
			}
			
			if (!$terminated) $this->index = $this->length;
			
			if (!$dec_found) {
				if (count($num) == 0) return $this->error();
				$n = intval(implode($num));
				return PastelGeneratedCode::buildInteger($this->globals, $n);
			}
			// TODO: are things like 1. valid?
			if (count($dec) == 0) {
				if (count($num) == 0) {
					$i--;
					return $this->error();
				}
				array_push($dec, '0');
			}
			return PastelGeneratedCode::buildFloat($this->globals, floatval(implode($num) . '.' . implode($dec)));
		}
		
		private function get_unicode_char($four_digit_hex) {
			if (isset($this->unicode_by_hex[$four_digit_hex])) {
				return $this->unicode_by_hex[$four_digit_hex];
			}
			
			$hack = json_decode('{"u": "\\u' . $four_digit_hex . '"}', true);
			if ($hack === null) return $this->error();
			$c = $hack['u'];
			$this->unicode_by_hex[$four_digit_hex] = $c;
			return $c;
		}
		
		private function parse_string() {
			$s = $this->parse_string_raw();
			if ($s === null) return $this->error();
			return PastelGeneratedCode::buildString($this->globals, $s);
		}
		
		private function parse_string_raw() {
			$output = array();
			if (!$this->pop('"')) return $this->error();
			for ($i = $this->index; $i < $this->length; ++$i) {
				$c = $this->text[$i];
				switch ($c) {
					case '"':
						$this->index = $i + 1;
						return implode($output);
					case "\\":
						if ($i + 1 == $this->length) {
							return $this->error();
						}
						$i++;
						switch ($this->text[$i]) {
							case "\\": array_push($output, "\\"); break;
							case 't': array_push($output, "\t"); break;
							case 'n': array_push($output, "\n"); break;
							case 'r': array_push($output, "\r"); break;
							case '0': array_push($output, "\0"); break;
							case '"': array_push($output, '"'); break;
							case 'u':
								if ($i + 4 > $this->length) return $this->error();
								$c = $this->get_unicode_char(substr($this->text, $i + 1, 4));
								if ($c === null) return $this->error();
								array_push($output, $c);
								$i += 4;
								break;
							default:
								return $this->error();
						}
						break;
					default:
						array_push($output, $c);
						break;
				}
			}
			return $this->error();
		}
		
		private function parse_object() {
			if (!$this->pop('{')) return $this->error();
			$this->skip_whitespace();
			$keys = array();
			$values_by_key = array();
			while (!$this->pop('}')) {
				if (count($keys) > 0) {
					if (!$this->pop(',')) return $this->error();
					$this->skip_whitespace();
				}
				$key = $this->parse_string_raw();
				if ($key === null) return $this->error();
				$this->skip_whitespace();
				if (!$this->pop(':')) return $this->error();
				$this->skip_whitespace();
				$value = $this->parse_thing();
				if ($value === null) return $this->error();
				$this->skip_whitespace();
				
				if (!isset($values_by_key[$key])) {
					array_push($keys, $key);
				}
				$values_by_key[$key] = $value;
			}
			
			$values = array();
			foreach ($keys as $key) {
				array_push($values, $values_by_key[$key]);
			}
			return PastelGeneratedCode::buildStringDictionary($this->globals, pastelWrapList($keys), pastelWrapList($values));
		}
		
		private function parse_list() {
			if (!$this->pop('[')) return $this->error();
			$this->skip_whitespace();
			$output = array();
			while (!$this->pop(']')) {
				if (count($output) > 0) {
					if (!$this->pop(',')) return $this->error();
					$this->skip_whitespace();
				}
				$item = $this->parse_thing();
				if ($item === null) return $this->error();
				array_push($output, $item);
				$this->skip_whitespace();
			}
			return PastselGeneratedCode::buildList(pastelWrapList($output));
		}
		
		private function pop($value) {
			if ($this->is_next($value)) {
				$this->index += strlen($value);
				return true;
			}
			return false;
		}
		
		private function is_next($value) {
			if ($this->index + strlen($value) > $this->length) return false;
			for ($i = 0; $i < strlen($value); ++$i) {
				if ($this->text[$i + $this->index] != $value[$i]) return false;
			}
			return true;
		}
		
		private function skip_whitespace() {
			while ($this->index < $this->length) {
				switch ($this->text[$this->index]) {
					case ' ':
					case "\t":
					case "\r":
					case "\n":
						$this->index++;
						break;
					default:
						return;
				}
			}
		}
		
		private function error() {
			if ($this->has_error) return null;
			$this->error_at = $this->index;
			$this->has_error = true;
			return null;
		}
		
	}


?>