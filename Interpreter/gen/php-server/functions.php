<?php

	// ensures array's pointer behavior behaves according to Pastel standards.
	class PastelPtrArray {
		var $arr = array();
	}
	function _pastelWrapValue($value) { $o = new PastelPtrArray(); $o->arr = $value; return $o; }
	// redundant-but-pleasantly-named helper methods for external callers
	function pastelWrapList($arr) { return _pastelWrapValue($arr); }
	function pastelWrapDictionary($arr) { return _pastelWrapValue($arr); }

	class PastelGeneratedCode {
		private static function PST_assignIndexHack($list, $index, $value) {
			$list->arr[$index] = $value;
		}

		private static function PST_stringIndexOf($haystack, $needle, $index) {
			$o = strpos($haystack, $needle, $index);
			if ($o === false) return -1;
			return $o;
		}

		private static function PST_dictGetKeys($d, $isIntDict) {
			$keys = new PastelPtrArray();
			foreach ($d->arr as $k => $ignore) {
				array_push($keys->arr, $isIntDict ? intval(substr($k, 1)) : $k);
			}
			return $keys;
		}

		public static $PST_intBuffer16 = null;

		private static function PST_stringEndsWith($haystack, $needle) {
			$nLen = strlen($needle);
			$hLen = strlen($haystack);
			if ($nLen === 0) return true;
			if ($hLen <= $nLen) return $haystack === $needle;
			$hOffset = $hLen - $nLen;
			for ($i = 0; $i < $nLen; ++$i) {
				if ($haystack[$hOffset + $i] !== $needle[$i]) return false;
			}
			return true;
		}

		private static function PST_stringStartsWith($haystack, $needle) {
			$nLen = strlen($needle);
			$hLen = strlen($haystack);
			if ($nLen === 0) return true;
			if ($hLen <= $nLen) return $haystack === $needle;
			for ($i = 0; $i < $nLen; ++$i) {
				if ($haystack[$i] !== $needle[$i]) return false;
			}
			return true;
		}

		private static function PST_reverseArray($a) {
			$a->arr = array_reverse($a->arr);
		}

		private static function PST_createFunctionPointer($name) {
			throw new Exception("Not implemented");
		}

		private static function PST_isValidInteger($s) {
			$length = strlen($s);
			if ($length == 0 || $s === '-') return false;
			$index = $s[0] === '-' ? 1 : 0;
			$n0 = ord('0');
			$n9 = $n0 + 9;
			while ($index < $length) {
				$c = ord($s[$i]);
				if ($c < $n0 || $c > $n9) return false;
			}
			return true;
		}

		private static function PST_tryParseFloat($s, $outValue) {
			$s = trim($s);
			$f = floatval($s);
			$check = '' . $f;
			$valid = $s === '' . $f;
			$outValue->arr[0] = $valid ? 1.0 : -1.0;
			if ($valid) {
				$outValue->arr[1] = $f;
			}
		}

		private static function PST_sortedCopyOfStringArray($strs) {
			$o = new PastelPtrArray();
			$o->arr = array_merge($strs->arr, array());
			sort($o->arr);
			return $o;
		}

		public static function addLiteralImpl($vm, $row, $stringArg) {
			$g = $vm->globals;
			$type = $row->arr[0];
			if (($type == 1)) {
				array_push($vm->metadata->literalTableBuilder->arr, $g->valueNull);
			} else if (($type == 2)) {
				array_push($vm->metadata->literalTableBuilder->arr, self::buildBoolean($g, ($row->arr[1] == 1)));
			} else if (($type == 3)) {
				array_push($vm->metadata->literalTableBuilder->arr, self::buildInteger($g, $row->arr[1]));
			} else if (($type == 4)) {
				array_push($vm->metadata->literalTableBuilder->arr, self::buildFloat($g, floatval($stringArg)));
			} else if (($type == 5)) {
				array_push($vm->metadata->literalTableBuilder->arr, self::buildCommonString($g, $stringArg));
			} else if (($type == 9)) {
				$index = count($vm->metadata->literalTableBuilder->arr);
				array_push($vm->metadata->literalTableBuilder->arr, self::buildCommonString($g, $stringArg));
				$vm->metadata->invFunctionNameLiterals->arr[$stringArg] = $index;
			} else if (($type == 10)) {
				$cv = new ClassValue(false, $row->arr[1]);
				array_push($vm->metadata->literalTableBuilder->arr, new Value(10, $cv));
			}
			return 0;
		}

		public static function addNameImpl($vm, $nameValue) {
			$index = count($vm->metadata->identifiersBuilder->arr);
			$vm->metadata->invIdentifiers->arr[$nameValue] = $index;
			array_push($vm->metadata->identifiersBuilder->arr, $nameValue);
			if ("length" === $nameValue) {
				$vm->metadata->lengthId = $index;
			}
			return 0;
		}

		public static function addToList($list, $item) {
			array_push($list->list->arr, $item);
			$list->size += 1;
		}

		public static function applyDebugSymbolData($vm, $opArgs, $stringData, $recentlyDefinedFunction) {
			return 0;
		}

		public static function buildBoolean($g, $value) {
			if ($value) {
				return $g->boolTrue;
			}
			return $g->boolFalse;
		}

		public static function buildCommonString($g, $s) {
			$value = null;
			$value = isset($g->commonStrings->arr[$s]) ? $g->commonStrings->arr[$s] : (null);
			if (($value == null)) {
				$value = self::buildString($g, $s);
				$g->commonStrings->arr[$s] = $value;
			}
			return $value;
		}

		public static function buildFloat($g, $value) {
			if (($value == 0.0)) {
				return $g->floatZero;
			}
			if (($value == 1.0)) {
				return $g->floatOne;
			}
			return new Value(4, $value);
		}

		public static function buildInteger($g, $num) {
			if (($num < 0)) {
				if (($num > -257)) {
					return $g->negativeIntegers->arr[-$num];
				}
			} else if (($num < 2049)) {
				return $g->positiveIntegers->arr[$num];
			}
			return new Value(3, $num);
		}

		public static function buildList($valueList) {
			return self::buildListWithType(null, $valueList);
		}

		public static function buildListWithType($type, $valueList) {
			return new Value(6, new ListImpl($type, count($valueList->arr), $valueList));
		}

		public static function buildNull($globals) {
			return $globals->valueNull;
		}

		public static function buildRelayObj($type, $iarg1, $iarg2, $iarg3, $farg1, $sarg1) {
			return new PlatformRelayObject($type, $iarg1, $iarg2, $iarg3, $farg1, $sarg1);
		}

		public static function buildString($g, $s) {
			if ((strlen($s) == 0)) {
				return $g->stringEmpty;
			}
			return new Value(5, $s);
		}

		public static function buildStringDictionary($globals, $stringKeys, $values) {
			$size = count($stringKeys->arr);
			$d = new DictImpl($size, 5, 0, null, new PastelPtrArray(), new PastelPtrArray(), new PastelPtrArray(), new PastelPtrArray());
			$k = null;
			$i = 0;
			while (($i < $size)) {
				$k = $stringKeys->arr[$i];
				if (isset($d->stringToIndex->arr[$k])) {
					self::PST_assignIndexHack($d->values, $d->stringToIndex->arr[$k], $values->arr[$i]);
				} else {
					$d->stringToIndex->arr[$k] = count($d->values->arr);
					array_push($d->values->arr, $values->arr[$i]);
					array_push($d->keys->arr, self::buildString($globals, $k));
				}
				$i += 1;
			}
			$d->size = count($d->values->arr);
			return new Value(7, $d);
		}

		public static function canAssignGenericToGeneric($vm, $gen1, $gen1Index, $gen2, $gen2Index, $newIndexOut) {
			if (($gen2 == null)) {
				return true;
			}
			if (($gen1 == null)) {
				return false;
			}
			$t1 = $gen1->arr[$gen1Index];
			$t2 = $gen2->arr[$gen2Index];
			switch ($t1) {
				case 0:
					$newIndexOut->arr[0] = ($gen1Index + 1);
					$newIndexOut->arr[1] = ($gen2Index + 2);
					return ($t2 == $t1);
				case 1:
					$newIndexOut->arr[0] = ($gen1Index + 1);
					$newIndexOut->arr[1] = ($gen2Index + 2);
					return ($t2 == $t1);
				case 2:
					$newIndexOut->arr[0] = ($gen1Index + 1);
					$newIndexOut->arr[1] = ($gen2Index + 2);
					return ($t2 == $t1);
				case 4:
					$newIndexOut->arr[0] = ($gen1Index + 1);
					$newIndexOut->arr[1] = ($gen2Index + 2);
					return ($t2 == $t1);
				case 5:
					$newIndexOut->arr[0] = ($gen1Index + 1);
					$newIndexOut->arr[1] = ($gen2Index + 2);
					return ($t2 == $t1);
				case 10:
					$newIndexOut->arr[0] = ($gen1Index + 1);
					$newIndexOut->arr[1] = ($gen2Index + 2);
					return ($t2 == $t1);
				case 3:
					$newIndexOut->arr[0] = ($gen1Index + 1);
					$newIndexOut->arr[1] = ($gen2Index + 2);
					return (($t2 == 3) || ($t2 == 4));
				case 8:
					$newIndexOut->arr[0] = ($gen1Index + 1);
					$newIndexOut->arr[1] = ($gen2Index + 2);
					if (($t2 != 8)) {
						return false;
					}
					$c1 = $gen1->arr[($gen1Index + 1)];
					$c2 = $gen2->arr[($gen2Index + 1)];
					if (($c1 == $c2)) {
						return true;
					}
					return self::isClassASubclassOf($vm, $c1, $c2);
				case 6:
					if (($t2 != 6)) {
						return false;
					}
					return self::canAssignGenericToGeneric($vm, $gen1, ($gen1Index + 1), $gen2, ($gen2Index + 1), $newIndexOut);
				case 7:
					if (($t2 != 7)) {
						return false;
					}
					if (!self::canAssignGenericToGeneric($vm, $gen1, ($gen1Index + 1), $gen2, ($gen2Index + 1), $newIndexOut)) {
						return false;
					}
					return self::canAssignGenericToGeneric($vm, $gen1, $newIndexOut->arr[0], $gen2, $newIndexOut->arr[1], $newIndexOut);
				case 9:
					if (($t2 != 9)) {
						return false;
					}
					return false;
				default:
					return false;
			}
		}

		public static function canAssignTypeToGeneric($vm, $value, $generics, $genericIndex) {
			switch ($value->type) {
				case 1:
					switch ($generics->arr[$genericIndex]) {
						case 5:
							return $value;
						case 8:
							return $value;
						case 10:
							return $value;
						case 9:
							return $value;
						case 6:
							return $value;
						case 7:
							return $value;
					}
					return null;
				case 2:
					if (($generics->arr[$genericIndex] == $value->type)) {
						return $value;
					}
					return null;
				case 5:
					if (($generics->arr[$genericIndex] == $value->type)) {
						return $value;
					}
					return null;
				case 10:
					if (($generics->arr[$genericIndex] == $value->type)) {
						return $value;
					}
					return null;
				case 3:
					if (($generics->arr[$genericIndex] == 3)) {
						return $value;
					}
					if (($generics->arr[$genericIndex] == 4)) {
						return self::buildFloat($vm->globals, (0.0 + $value->internalValue));
					}
					return null;
				case 4:
					if (($generics->arr[$genericIndex] == 4)) {
						return $value;
					}
					return null;
				case 6:
					$list = $value->internalValue;
					$listType = $list->type;
					$genericIndex += 1;
					if (($listType == null)) {
						if ((($generics->arr[$genericIndex] == 1) || ($generics->arr[$genericIndex] == 0))) {
							return $value;
						}
						return null;
					}
					$i = 0;
					while (($i < count($listType->arr))) {
						if (($listType->arr[$i] != $generics->arr[($genericIndex + $i)])) {
							return null;
						}
						$i += 1;
					}
					return $value;
				case 7:
					$dict = $value->internalValue;
					$j = $genericIndex;
					switch ($dict->keyType) {
						case 3:
							if (($generics->arr[1] == $dict->keyType)) {
								$j += 2;
							} else {
								return null;
							}
							break;
						case 5:
							if (($generics->arr[1] == $dict->keyType)) {
								$j += 2;
							} else {
								return null;
							}
							break;
						case 8:
							if (($generics->arr[1] == 8)) {
								$j += 3;
							} else {
								return null;
							}
							break;
					}
					$valueType = $dict->valueType;
					if (($valueType == null)) {
						if ((($generics->arr[$j] == 0) || ($generics->arr[$j] == 1))) {
							return $value;
						}
						return null;
					}
					$k = 0;
					while (($k < count($valueType->arr))) {
						if (($valueType->arr[$k] != $generics->arr[($j + $k)])) {
							return null;
						}
						$k += 1;
					}
					return $value;
				case 8:
					if (($generics->arr[$genericIndex] == 8)) {
						$targetClassId = $generics->arr[($genericIndex + 1)];
						$givenClassId = ($value->internalValue)->classId;
						if (($targetClassId == $givenClassId)) {
							return $value;
						}
						if (self::isClassASubclassOf($vm, $givenClassId, $targetClassId)) {
							return $value;
						}
					}
					return null;
			}
			return null;
		}

		public static function canonicalizeAngle($a) {
			$twopi = 6.28318530717958;
			$a = ($a % $twopi);
			if (($a < 0)) {
				$a += $twopi;
			}
			return $a;
		}

		public static function canonicalizeListSliceArgs($outParams, $beginValue, $endValue, $beginIndex, $endIndex, $stepAmount, $length, $isForward) {
			if (($beginValue == null)) {
				if ($isForward) {
					$beginIndex = 0;
				} else {
					$beginIndex = ($length - 1);
				}
			}
			if (($endValue == null)) {
				if ($isForward) {
					$endIndex = $length;
				} else {
					$endIndex = (-1 - $length);
				}
			}
			if (($beginIndex < 0)) {
				$beginIndex += $length;
			}
			if (($endIndex < 0)) {
				$endIndex += $length;
			}
			if ((($beginIndex == 0) && ($endIndex == $length) && ($stepAmount == 1))) {
				return 2;
			}
			if ($isForward) {
				if (($beginIndex >= $length)) {
					return 0;
				}
				if (($beginIndex < 0)) {
					return 3;
				}
				if (($endIndex < $beginIndex)) {
					return 4;
				}
				if (($beginIndex == $endIndex)) {
					return 0;
				}
				if (($endIndex > $length)) {
					$endIndex = $length;
				}
			} else {
				if (($beginIndex < 0)) {
					return 0;
				}
				if (($beginIndex >= $length)) {
					return 3;
				}
				if (($endIndex > $beginIndex)) {
					return 4;
				}
				if (($beginIndex == $endIndex)) {
					return 0;
				}
				if (($endIndex < -1)) {
					$endIndex = -1;
				}
			}
			$outParams->arr[0] = $beginIndex;
			$outParams->arr[1] = $endIndex;
			return 1;
		}

		public static function classIdToString($vm, $classId) {
			return $vm->metadata->classTable->arr[$classId]->fullyQualifiedName;
		}

		public static function clearList($a) {
			$a->list->arr = array();
			$a->size = 0;
			return 0;
		}

		public static function cloneDictionary($original, $clone) {
			$type = $original->keyType;
			$i = 0;
			$size = $original->size;
			$kInt = 0;
			$kString = null;
			if (($clone == null)) {
				$clone = new DictImpl(0, $type, $original->keyClassId, $original->valueType, new PastelPtrArray(), new PastelPtrArray(), new PastelPtrArray(), new PastelPtrArray());
				if (($type == 5)) {
					while (($i < $size)) {
						$clone->stringToIndex->arr[$original->keys->arr[$i]->internalValue] = $i;
						$i += 1;
					}
				} else {
					while (($i < $size)) {
						if (($type == 8)) {
							$kInt = ($original->keys->arr[$i]->internalValue)->objectId;
						} else {
							$kInt = $original->keys->arr[$i]->internalValue;
						}
						$clone->intToIndex->arr['i'.$kInt] = $i;
						$i += 1;
					}
				}
				$i = 0;
				while (($i < $size)) {
					array_push($clone->keys->arr, $original->keys->arr[$i]);
					array_push($clone->values->arr, $original->values->arr[$i]);
					$i += 1;
				}
			} else {
				$i = 0;
				while (($i < $size)) {
					if (($type == 5)) {
						$kString = $original->keys->arr[$i]->internalValue;
						if (isset($clone->stringToIndex->arr[$kString])) {
							self::PST_assignIndexHack($clone->values, $clone->stringToIndex->arr[$kString], $original->values->arr[$i]);
						} else {
							$clone->stringToIndex->arr[$kString] = count($clone->values->arr);
							array_push($clone->values->arr, $original->values->arr[$i]);
							array_push($clone->keys->arr, $original->keys->arr[$i]);
						}
					} else {
						if (($type == 3)) {
							$kInt = $original->keys->arr[$i]->internalValue;
						} else {
							$kInt = ($original->keys->arr[$i]->internalValue)->objectId;
						}
						if (isset($clone->intToIndex->arr['i'.$kInt])) {
							self::PST_assignIndexHack($clone->values, $clone->intToIndex->arr['i'.$kInt], $original->values->arr[$i]);
						} else {
							$clone->intToIndex->arr['i'.$kInt] = count($clone->values->arr);
							array_push($clone->values->arr, $original->values->arr[$i]);
							array_push($clone->keys->arr, $original->keys->arr[$i]);
						}
					}
					$i += 1;
				}
			}
			$clone->size = (count($clone->intToIndex->arr) + count($clone->stringToIndex->arr));
			return $clone;
		}

		public static function createInstanceType($classId) {
			$o = pastelWrapList(array_fill(0, 2, 0));
			$o->arr[0] = 8;
			$o->arr[1] = $classId;
			return $o;
		}

		public static function createVm($rawByteCode, $resourceManifest) {
			$globals = self::initializeConstantValues();
			$resources = self::resourceManagerInitialize($globals, $resourceManifest);
			$byteCode = self::initializeByteCode($rawByteCode);
			$localsStack = pastelWrapList(array_fill(0, 10, null));
			$localsStackSet = pastelWrapList(array_fill(0, 10, 0));
			$i = 0;
			$i = (count($localsStack->arr) - 1);
			while (($i >= 0)) {
				$localsStack->arr[$i] = null;
				$localsStackSet->arr[$i] = 0;
				$i -= 1;
			}
			$stack = new StackFrame(0, 1, 0, 0, null, false, null, 0, 0, 1, 0, null, null, null);
			$executionContext = new ExecutionContext(0, $stack, 0, 100, pastelWrapList(array_fill(0, 100, null)), $localsStack, $localsStackSet, 1, 0, false, null, false, 0, null);
			$executionContexts = new PastelPtrArray();
			$executionContexts->arr['i0'] = $executionContext;
			$vm = new VmContext($executionContexts, $executionContext->id, $byteCode, new SymbolData(pastelWrapList(array_fill(0, count($byteCode->ops->arr), null)), null, new PastelPtrArray(), null, null, new PastelPtrArray(), new PastelPtrArray()), new VmMetadata(null, new PastelPtrArray(), new PastelPtrArray(), null, new PastelPtrArray(), null, new PastelPtrArray(), null, new PastelPtrArray(), pastelWrapList(array_fill(0, 100, null)), pastelWrapList(array_fill(0, 100, null)), new PastelPtrArray(), null, new PastelPtrArray(), -1, pastelWrapList(array_fill(0, 10, 0)), 0, null, null, new MagicNumbers(0, 0, 0), new PastelPtrArray(), new PastelPtrArray(), null), 0, false, new PastelPtrArray(), null, $resources, new PastelPtrArray(), new VmEnvironment(pastelWrapList(array_fill(0, 0, null)), false, null, null), new NamedCallbackStore(new PastelPtrArray(), new PastelPtrArray()), $globals, $globals->valueNull, $globals->boolTrue, $globals->boolFalse);
			return $vm;
		}

		public static function debuggerClearBreakpoint($vm, $id) {
			return 0;
		}

		public static function debuggerFindPcForLine($vm, $path, $line) {
			return -1;
		}

		public static function debuggerSetBreakpoint($vm, $path, $line) {
			return -1;
		}

		public static function debugSetStepOverBreakpoint($vm) {
			return false;
		}

		public static function defOriginalCodeImpl($vm, $row, $fileContents) {
			$fileId = $row->arr[0];
			$codeLookup = $vm->symbolData->sourceCodeBuilder;
			while ((count($codeLookup->arr) <= $fileId)) {
				array_push($codeLookup->arr, null);
			}
			$codeLookup->arr[$fileId] = $fileContents;
			return 0;
		}

		public static function dictKeyInfoToString($vm, $dict) {
			if (($dict->keyType == 5)) {
				return "string";
			}
			if (($dict->keyType == 3)) {
				return "int";
			}
			if (($dict->keyClassId == 0)) {
				return "instance";
			}
			return self::classIdToString($vm, $dict->keyClassId);
		}

		public static function doEqualityComparisonAndReturnCode($a, $b) {
			$leftType = $a->type;
			$rightType = $b->type;
			if (($leftType == $rightType)) {
				$output = 0;
				switch ($leftType) {
					case 1:
						$output = 1;
						break;
					case 3:
						if (($a->internalValue == $b->internalValue)) {
							$output = 1;
						}
						break;
					case 4:
						if (($a->internalValue == $b->internalValue)) {
							$output = 1;
						}
						break;
					case 2:
						if (($a->internalValue == $b->internalValue)) {
							$output = 1;
						}
						break;
					case 5:
						if (($a->internalValue == $b->internalValue)) {
							$output = 1;
						}
						break;
					case 6:
						if (($a->internalValue == $b->internalValue)) {
							$output = 1;
						}
						break;
					case 7:
						if (($a->internalValue == $b->internalValue)) {
							$output = 1;
						}
						break;
					case 8:
						if (($a->internalValue == $b->internalValue)) {
							$output = 1;
						}
						break;
					case 9:
						$f1 = $a->internalValue;
						$f2 = $b->internalValue;
						if (($f1->functionId == $f2->functionId)) {
							if ((($f1->type == 2) || ($f1->type == 4))) {
								if ((self::doEqualityComparisonAndReturnCode($f1->context, $f2->context) == 1)) {
									$output = 1;
								}
							} else {
								$output = 1;
							}
						}
						break;
					case 10:
						$c1 = $a->internalValue;
						$c2 = $b->internalValue;
						if (($c1->classId == $c2->classId)) {
							$output = 1;
						}
						break;
					default:
						$output = 2;
						break;
				}
				return $output;
			}
			if (($rightType == 1)) {
				return 0;
			}
			if ((($leftType == 3) && ($rightType == 4))) {
				if (($a->internalValue == $b->internalValue)) {
					return 1;
				}
			} else if ((($leftType == 4) && ($rightType == 3))) {
				if (($a->internalValue == $b->internalValue)) {
					return 1;
				}
			}
			return 0;
		}

		public static function doExponentMath($globals, $b, $e, $preferInt) {
			if (($e == 0.0)) {
				if ($preferInt) {
					return $globals->intOne;
				}
				return $globals->floatOne;
			}
			if (($b == 0.0)) {
				if ($preferInt) {
					return $globals->intZero;
				}
				return $globals->floatZero;
			}
			$r = 0.0;
			if (($b < 0)) {
				if ((($e >= 0) && ($e < 1))) {
					return null;
				}
				if ((($e % 1.0) == 0.0)) {
					$eInt = intval($e);
					$r = (0.0 + pow($b, $eInt));
				} else {
					return null;
				}
			} else {
				$r = pow($b, $e);
			}
			if ($preferInt) {
				$r = self::fixFuzzyFloatPrecision($r);
				if ((($r % 1.0) == 0.0)) {
					return self::buildInteger($globals, intval($r));
				}
			}
			return self::buildFloat($globals, $r);
		}

		public static function encodeBreakpointData($vm, $breakpoint, $pc) {
			return null;
		}

		public static function errorResult($error) {
			return new InterpreterResult(3, $error, 0.0, 0, false, "");
		}

		public static function EX_AssertionFailed($ec, $exMsg) {
			return self::generateException2($ec, 2, $exMsg);
		}

		public static function EX_DivisionByZero($ec, $exMsg) {
			return self::generateException2($ec, 3, $exMsg);
		}

		public static function EX_Fatal($ec, $exMsg) {
			return self::generateException2($ec, 0, $exMsg);
		}

		public static function EX_IndexOutOfRange($ec, $exMsg) {
			return self::generateException2($ec, 4, $exMsg);
		}

		public static function EX_InvalidArgument($ec, $exMsg) {
			return self::generateException2($ec, 5, $exMsg);
		}

		public static function EX_InvalidAssignment($ec, $exMsg) {
			return self::generateException2($ec, 6, $exMsg);
		}

		public static function EX_InvalidInvocation($ec, $exMsg) {
			return self::generateException2($ec, 7, $exMsg);
		}

		public static function EX_InvalidKey($ec, $exMsg) {
			return self::generateException2($ec, 8, $exMsg);
		}

		public static function EX_KeyNotFound($ec, $exMsg) {
			return self::generateException2($ec, 9, $exMsg);
		}

		public static function EX_NullReference($ec, $exMsg) {
			return self::generateException2($ec, 10, $exMsg);
		}

		public static function EX_UnassignedVariable($ec, $exMsg) {
			return self::generateException2($ec, 11, $exMsg);
		}

		public static function EX_UnknownField($ec, $exMsg) {
			return self::generateException2($ec, 12, $exMsg);
		}

		public static function EX_UnsupportedOperation($ec, $exMsg) {
			return self::generateException2($ec, 13, $exMsg);
		}

		public static function finalizeInitializationImpl($vm, $projectId, $localeCount) {
			$vm->symbolData->sourceCode = pastelWrapList($vm->symbolData->sourceCodeBuilder->arr);
			$vm->symbolData->sourceCodeBuilder = null;
			$vm->metadata->magicNumbers->totalLocaleCount = $localeCount;
			$vm->metadata->identifiers = pastelWrapList($vm->metadata->identifiersBuilder->arr);
			$vm->metadata->literalTable = pastelWrapList($vm->metadata->literalTableBuilder->arr);
			$vm->metadata->globalNameIdToPrimitiveMethodName = self::primitiveMethodsInitializeLookup($vm->metadata->invIdentifiers);
			$vm->funcArgs = pastelWrapList(array_fill(0, count($vm->metadata->identifiers->arr), null));
			$vm->metadata->projectId = $projectId;
			$vm->metadata->identifiersBuilder = null;
			$vm->metadata->literalTableBuilder = null;
			$vm->initializationComplete = true;
			return 0;
		}

		public static function fixFuzzyFloatPrecision($x) {
			if ((($x % 1) != 0)) {
				$u = ($x % 1);
				if (($u < 0)) {
					$u += 1.0;
				}
				$roundDown = false;
				if (($u > 0.9999999999)) {
					$roundDown = true;
					$x += 0.1;
				} else if (($u < 0.00000000002250000000)) {
					$roundDown = true;
				}
				if ($roundDown) {
					if ((false || ($x > 0))) {
						$x = (intval($x) + 0.0);
					} else {
						$x = (intval($x) - 1.0);
					}
				}
			}
			return $x;
		}

		public static function generateEsfData($byteCodeLength, $esfArgs) {
			$output = pastelWrapList(array_fill(0, $byteCodeLength, null));
			$esfTokenStack = new PastelPtrArray();
			$esfTokenStackTop = null;
			$esfArgIterator = 0;
			$esfArgLength = count($esfArgs->arr);
			$j = 0;
			$pc = 0;
			while (($pc < $byteCodeLength)) {
				if ((($esfArgIterator < $esfArgLength) && ($pc == $esfArgs->arr[$esfArgIterator]))) {
					$esfTokenStackTop = pastelWrapList(array_fill(0, 2, 0));
					$j = 1;
					while (($j < 3)) {
						$esfTokenStackTop->arr[($j - 1)] = $esfArgs->arr[($esfArgIterator + $j)];
						$j += 1;
					}
					array_push($esfTokenStack->arr, $esfTokenStackTop);
					$esfArgIterator += 3;
				}
				while ((($esfTokenStackTop != null) && ($esfTokenStackTop->arr[1] <= $pc))) {
					array_pop($esfTokenStack->arr);
					if ((count($esfTokenStack->arr) == 0)) {
						$esfTokenStackTop = null;
					} else {
						$esfTokenStackTop = $esfTokenStack->arr[(count($esfTokenStack->arr) - 1)];
					}
				}
				$output->arr[$pc] = $esfTokenStackTop;
				$pc += 1;
			}
			return $output;
		}

		public static function generateException($vm, $stack, $pc, $valueStackSize, $ec, $type, $message) {
			$ec->currentValueStackSize = $valueStackSize;
			$stack->pc = $pc;
			$mn = $vm->metadata->magicNumbers;
			$generateExceptionFunctionId = $mn->coreGenerateExceptionFunctionId;
			$functionInfo = $vm->metadata->functionTable->arr[$generateExceptionFunctionId];
			$pc = $functionInfo->pc;
			if ((count($ec->localsStack->arr) <= ($functionInfo->localsSize + $stack->localsStackOffsetEnd))) {
				self::increaseLocalsStackCapacity($ec, $functionInfo->localsSize);
			}
			$localsIndex = $stack->localsStackOffsetEnd;
			$localsStackSetToken = ($ec->localsStackSetToken + 1);
			$ec->localsStackSetToken = $localsStackSetToken;
			self::PST_assignIndexHack($ec->localsStack, $localsIndex, self::buildInteger($vm->globals, $type));
			self::PST_assignIndexHack($ec->localsStack, ($localsIndex + 1), self::buildString($vm->globals, $message));
			self::PST_assignIndexHack($ec->localsStackSet, $localsIndex, $localsStackSetToken);
			self::PST_assignIndexHack($ec->localsStackSet, ($localsIndex + 1), $localsStackSetToken);
			$ec->stackTop = new StackFrame(($pc + 1), $localsStackSetToken, $stack->localsStackOffsetEnd, ($stack->localsStackOffsetEnd + $functionInfo->localsSize), $stack, false, null, $valueStackSize, 0, ($stack->depth + 1), 0, null, null, null);
			return new InterpreterResult(5, null, 0.0, 0, false, "");
		}

		public static function generateException2($ec, $exceptionType, $exMsg) {
			$ec->activeInterrupt = new Interrupt(1, $exceptionType, $exMsg, 0.0, null);
			return true;
		}

		public static function generatePrimitiveMethodReference($lookup, $globalNameId, $context) {
			$functionId = self::resolvePrimitiveMethodName2($lookup, $context->type, $globalNameId);
			if (($functionId < 0)) {
				return null;
			}
			return new Value(9, new FunctionPointer(4, $context, 0, $functionId, null));
		}

		public static function generateTokenListFromPcs($vm, $pcs) {
			$output = new PastelPtrArray();
			$tokensByPc = $vm->symbolData->tokenData;
			$token = null;
			$i = 0;
			while (($i < count($pcs->arr))) {
				$localTokens = $tokensByPc->arr[$pcs->arr[$i]];
				if (($localTokens == null)) {
					if ((count($output->arr) > 0)) {
						array_push($output->arr, null);
					}
				} else {
					$token = $localTokens->arr[0];
					array_push($output->arr, $token);
				}
				$i += 1;
			}
			return $output;
		}

		public static function getBinaryOpFromId($id) {
			switch ($id) {
				case 0:
					return "+";
				case 1:
					return "-";
				case 2:
					return "*";
				case 3:
					return "/";
				case 4:
					return "%";
				case 5:
					return "**";
				case 6:
					return "&";
				case 7:
					return "|";
				case 8:
					return "^";
				case 9:
					return "<<";
				case 10:
					return ">>";
				case 11:
					return "<";
				case 12:
					return "<=";
				case 13:
					return ">";
				case 14:
					return ">=";
				default:
					return "unknown";
			}
		}

		public static function getClassTable($vm, $classId) {
			$oldTable = $vm->metadata->classTable;
			$oldLength = count($oldTable->arr);
			if (($classId < $oldLength)) {
				return $oldTable;
			}
			$newLength = ($oldLength * 2);
			if (($classId >= $newLength)) {
				$newLength = ($classId + 100);
			}
			$newTable = pastelWrapList(array_fill(0, $newLength, null));
			$i = ($oldLength - 1);
			while (($i >= 0)) {
				$newTable->arr[$i] = $oldTable->arr[$i];
				$i -= 1;
			}
			$vm->metadata->classTable = $newTable;
			return $newTable;
		}

		public static function getExecutionContext($vm, $id) {
			if (isset($vm->executionContexts->arr['i'.$id])) {
				return $vm->executionContexts->arr['i'.$id];
			}
			return null;
		}

		public static function getExponentErrorMsg($vm, $b, $e) {
			return implode(array("Invalid values for exponent computation. Base: ", self::valueToString($vm, $b), ", Power: ", self::valueToString($vm, $e)));
		}

		public static function getFloat($num) {
			if (($num->type == 4)) {
				return $num->internalValue;
			}
			return ($num->internalValue + 0.0);
		}

		public static function getFunctionTable($vm, $functionId) {
			$oldTable = $vm->metadata->functionTable;
			$oldLength = count($oldTable->arr);
			if (($functionId < $oldLength)) {
				return $oldTable;
			}
			$newLength = ($oldLength * 2);
			if (($functionId >= $newLength)) {
				$newLength = ($functionId + 100);
			}
			$newTable = pastelWrapList(array_fill(0, $newLength, null));
			$i = 0;
			while (($i < $oldLength)) {
				$newTable->arr[$i] = $oldTable->arr[$i];
				$i += 1;
			}
			$vm->metadata->functionTable = $newTable;
			return $newTable;
		}

		public static function getItemFromList($list, $i) {
			return $list->list->arr[$i];
		}

		public static function getNamedCallbackId($vm, $scope, $functionName) {
			return self::getNamedCallbackIdImpl($vm, $scope, $functionName, false);
		}

		public static function getNamedCallbackIdImpl($vm, $scope, $functionName, $allocIfMissing) {
			$lookup = $vm->namedCallbacks->callbackIdLookup;
			$idsForScope = null;
			$idsForScope = isset($lookup->arr[$scope]) ? $lookup->arr[$scope] : (null);
			if (($idsForScope == null)) {
				$idsForScope = new PastelPtrArray();
				$lookup->arr[$scope] = $idsForScope;
			}
			$id = -1;
			$id = isset($idsForScope->arr[$functionName]) ? $idsForScope->arr[$functionName] : (-1);
			if ((($id == -1) && $allocIfMissing)) {
				$id = count($vm->namedCallbacks->callbacksById->arr);
				array_push($vm->namedCallbacks->callbacksById->arr, null);
				$idsForScope->arr[$functionName] = $id;
			}
			return $id;
		}

		public static function getNativeDataItem($objValue, $index) {
			$obj = $objValue->internalValue;
			return $obj->nativeData->arr[$index];
		}

		public static function getTypeFromId($id) {
			switch ($id) {
				case 1:
					return "null";
				case 2:
					return "boolean";
				case 3:
					return "integer";
				case 4:
					return "float";
				case 5:
					return "string";
				case 6:
					return "list";
				case 7:
					return "dictionary";
				case 8:
					return "instance";
				case 9:
					return "function";
			}
			return null;
		}

		public static function getVmReinvokeDelay($result) {
			return $result->reinvokeDelay;
		}

		public static function getVmResultAssemblyInfo($result) {
			return $result->loadAssemblyInformation;
		}

		public static function getVmResultExecId($result) {
			return $result->executionContextId;
		}

		public static function getVmResultStatus($result) {
			return $result->status;
		}

		public static function increaseListCapacity($list) {
		}

		public static function increaseLocalsStackCapacity($ec, $newScopeSize) {
			$oldLocals = $ec->localsStack;
			$oldSetIndicator = $ec->localsStackSet;
			$oldCapacity = count($oldLocals->arr);
			$newCapacity = (($oldCapacity * 2) + $newScopeSize);
			$newLocals = pastelWrapList(array_fill(0, $newCapacity, null));
			$newSetIndicator = pastelWrapList(array_fill(0, $newCapacity, 0));
			$i = 0;
			while (($i < $oldCapacity)) {
				$newLocals->arr[$i] = $oldLocals->arr[$i];
				$newSetIndicator->arr[$i] = $oldSetIndicator->arr[$i];
				$i += 1;
			}
			$ec->localsStack = $newLocals;
			$ec->localsStackSet = $newSetIndicator;
			return 0;
		}

		public static function initFileNameSymbolData($vm) {
			$symbolData = $vm->symbolData;
			if (($symbolData == null)) {
				return 0;
			}
			if (($symbolData->fileNameById == null)) {
				$i = 0;
				$filenames = pastelWrapList(array_fill(0, count($symbolData->sourceCode->arr), null));
				$fileIdByPath = new PastelPtrArray();
				$i = 0;
				while (($i < count($filenames->arr))) {
					$sourceCode = $symbolData->sourceCode->arr[$i];
					if (($sourceCode != null)) {
						$colon = self::PST_stringIndexOf($sourceCode, "\n", 0);
						if (($colon != -1)) {
							$filename = substr($sourceCode, 0, $colon);
							$filenames->arr[$i] = $filename;
							$fileIdByPath->arr[$filename] = $i;
						}
					}
					$i += 1;
				}
				$symbolData->fileNameById = $filenames;
				$symbolData->fileIdByName = $fileIdByPath;
			}
			return 0;
		}

		public static function initializeByteCode($raw) {
			$index = pastelWrapList(array_fill(0, 1, 0));
			$index->arr[0] = 0;
			$length = strlen($raw);
			$header = self::read_till($index, $raw, $length, "@");
			if (($header != "CRAYON")) {
			}
			$alphaNums = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
			$opCount = self::read_integer($index, $raw, $length, $alphaNums);
			$ops = pastelWrapList(array_fill(0, $opCount, 0));
			$iargs = pastelWrapList(array_fill(0, $opCount, null));
			$sargs = pastelWrapList(array_fill(0, $opCount, null));
			$c = " ";
			$argc = 0;
			$j = 0;
			$stringarg = null;
			$stringPresent = false;
			$iarg = 0;
			$iarglist = null;
			$i = 0;
			$i = 0;
			while (($i < $opCount)) {
				$c = $raw[$index->arr[0]];
				$index->arr[0] = ($index->arr[0] + 1);
				$argc = 0;
				$stringPresent = true;
				if (($c == "!")) {
					$argc = 1;
				} else if (($c == "&")) {
					$argc = 2;
				} else if (($c == "*")) {
					$argc = 3;
				} else {
					if (($c != "~")) {
						$stringPresent = false;
						$index->arr[0] = ($index->arr[0] - 1);
					}
					$argc = self::read_integer($index, $raw, $length, $alphaNums);
				}
				$iarglist = pastelWrapList(array_fill(0, ($argc - 1), 0));
				$j = 0;
				while (($j < $argc)) {
					$iarg = self::read_integer($index, $raw, $length, $alphaNums);
					if (($j == 0)) {
						$ops->arr[$i] = $iarg;
					} else {
						$iarglist->arr[($j - 1)] = $iarg;
					}
					$j += 1;
				}
				$iargs->arr[$i] = $iarglist;
				if ($stringPresent) {
					$stringarg = self::read_string($index, $raw, $length, $alphaNums);
				} else {
					$stringarg = null;
				}
				$sargs->arr[$i] = $stringarg;
				$i += 1;
			}
			$hasBreakpoint = pastelWrapList(array_fill(0, $opCount, false));
			$breakpointInfo = pastelWrapList(array_fill(0, $opCount, null));
			$i = 0;
			while (($i < $opCount)) {
				$hasBreakpoint->arr[$i] = false;
				$breakpointInfo->arr[$i] = null;
				$i += 1;
			}
			return new Code($ops, $iargs, $sargs, pastelWrapList(array_fill(0, $opCount, null)), pastelWrapList(array_fill(0, $opCount, null)), new VmDebugData($hasBreakpoint, $breakpointInfo, new PastelPtrArray(), 1, 0));
		}

		public static function initializeClass($pc, $vm, $args, $className) {
			$i = 0;
			$memberId = 0;
			$globalId = 0;
			$functionId = 0;
			$t = 0;
			$classId = $args->arr[0];
			$baseClassId = $args->arr[1];
			$globalNameId = $args->arr[2];
			$constructorFunctionId = $args->arr[3];
			$staticConstructorFunctionId = $args->arr[4];
			$staticInitializationState = 0;
			if (($staticConstructorFunctionId == -1)) {
				$staticInitializationState = 2;
			}
			$staticFieldCount = $args->arr[5];
			$assemblyId = $args->arr[6];
			$staticFields = pastelWrapList(array_fill(0, $staticFieldCount, null));
			$i = 0;
			while (($i < $staticFieldCount)) {
				$staticFields->arr[$i] = $vm->globals->valueNull;
				$i += 1;
			}
			$classInfo = new ClassInfo($classId, $globalNameId, $baseClassId, $assemblyId, $staticInitializationState, $staticFields, $staticConstructorFunctionId, $constructorFunctionId, 0, null, null, null, null, null, $vm->metadata->classMemberLocalizerBuilder->arr['i'.$classId], null, $className);
			$classTable = self::getClassTable($vm, $classId);
			$classTable->arr[$classId] = $classInfo;
			$classChain = new PastelPtrArray();
			array_push($classChain->arr, $classInfo);
			$classIdWalker = $baseClassId;
			while (($classIdWalker != -1)) {
				$walkerClass = $classTable->arr[$classIdWalker];
				array_push($classChain->arr, $walkerClass);
				$classIdWalker = $walkerClass->baseClassId;
			}
			$baseClass = null;
			if (($baseClassId != -1)) {
				$baseClass = $classChain->arr[1];
			}
			$functionIds = new PastelPtrArray();
			$fieldInitializationCommand = new PastelPtrArray();
			$fieldInitializationLiteral = new PastelPtrArray();
			$fieldAccessModifier = new PastelPtrArray();
			$globalNameIdToMemberId = new PastelPtrArray();
			if (($baseClass != null)) {
				$i = 0;
				while (($i < $baseClass->memberCount)) {
					array_push($functionIds->arr, $baseClass->functionIds->arr[$i]);
					array_push($fieldInitializationCommand->arr, $baseClass->fieldInitializationCommand->arr[$i]);
					array_push($fieldInitializationLiteral->arr, $baseClass->fieldInitializationLiteral->arr[$i]);
					array_push($fieldAccessModifier->arr, $baseClass->fieldAccessModifiers->arr[$i]);
					$i += 1;
				}
				$keys = self::PST_dictGetKeys($baseClass->globalIdToMemberId, true);
				$i = 0;
				while (($i < count($keys->arr))) {
					$t = $keys->arr[$i];
					$globalNameIdToMemberId->arr['i'.$t] = $baseClass->globalIdToMemberId->arr['i'.$t];
					$i += 1;
				}
				$keys = self::PST_dictGetKeys($baseClass->localeScopedNameIdToMemberId, true);
				$i = 0;
				while (($i < count($keys->arr))) {
					$t = $keys->arr[$i];
					$classInfo->localeScopedNameIdToMemberId->arr['i'.$t] = $baseClass->localeScopedNameIdToMemberId->arr['i'.$t];
					$i += 1;
				}
			}
			$accessModifier = 0;
			$i = 7;
			while (($i < count($args->arr))) {
				$memberId = $args->arr[($i + 1)];
				$globalId = $args->arr[($i + 2)];
				$accessModifier = $args->arr[($i + 5)];
				while (($memberId >= count($functionIds->arr))) {
					array_push($functionIds->arr, -1);
					array_push($fieldInitializationCommand->arr, -1);
					array_push($fieldInitializationLiteral->arr, null);
					array_push($fieldAccessModifier->arr, 0);
				}
				$globalNameIdToMemberId->arr['i'.$globalId] = $memberId;
				$fieldAccessModifier->arr[$memberId] = $accessModifier;
				if (($args->arr[$i] == 0)) {
					$fieldInitializationCommand->arr[$memberId] = $args->arr[($i + 3)];
					$t = $args->arr[($i + 4)];
					if (($t == -1)) {
						$fieldInitializationLiteral->arr[$memberId] = $vm->globals->valueNull;
					} else {
						$fieldInitializationLiteral->arr[$memberId] = $vm->metadata->literalTable->arr[$t];
					}
				} else {
					$functionId = $args->arr[($i + 3)];
					$functionIds->arr[$memberId] = $functionId;
				}
				$i += 6;
			}
			$classInfo->functionIds = pastelWrapList($functionIds->arr);
			$classInfo->fieldInitializationCommand = pastelWrapList($fieldInitializationCommand->arr);
			$classInfo->fieldInitializationLiteral = pastelWrapList($fieldInitializationLiteral->arr);
			$classInfo->fieldAccessModifiers = pastelWrapList($fieldAccessModifier->arr);
			$classInfo->memberCount = count($functionIds->arr);
			$classInfo->globalIdToMemberId = $globalNameIdToMemberId;
			$classInfo->typeInfo = pastelWrapList(array_fill(0, $classInfo->memberCount, null));
			if (($baseClass != null)) {
				$i = 0;
				while (($i < count($baseClass->typeInfo->arr))) {
					self::PST_assignIndexHack($classInfo->typeInfo, $i, $baseClass->typeInfo->arr[$i]);
					$i += 1;
				}
			}
			if ("Core.Exception" === $className) {
				$mn = $vm->metadata->magicNumbers;
				$mn->coreExceptionClassId = $classId;
			}
			return 0;
		}

		public static function initializeClassFieldTypeInfo($vm, $opCodeRow) {
			$classInfo = $vm->metadata->classTable->arr[$opCodeRow->arr[0]];
			$memberId = $opCodeRow->arr[1];
			$_len = count($opCodeRow->arr);
			$typeInfo = pastelWrapList(array_fill(0, ($_len - 2), 0));
			$i = 2;
			while (($i < $_len)) {
				$typeInfo->arr[($i - 2)] = $opCodeRow->arr[$i];
				$i += 1;
			}
			self::PST_assignIndexHack($classInfo->typeInfo, $memberId, $typeInfo);
			return 0;
		}

		public static function initializeConstantValues() {
			$pos = pastelWrapList(array_fill(0, 2049, null));
			$neg = pastelWrapList(array_fill(0, 257, null));
			$i = 0;
			while (($i < 2049)) {
				$pos->arr[$i] = new Value(3, $i);
				$i += 1;
			}
			$i = 1;
			while (($i < 257)) {
				$neg->arr[$i] = new Value(3, -$i);
				$i += 1;
			}
			$neg->arr[0] = $pos->arr[0];
			$globals = new VmGlobals(new Value(1, null), new Value(2, true), new Value(2, false), $pos->arr[0], $pos->arr[1], $neg->arr[1], new Value(4, 0.0), new Value(4, 1.0), new Value(5, ""), $pos, $neg, new PastelPtrArray(), pastelWrapList(array_fill(0, 1, 0)), pastelWrapList(array_fill(0, 1, 0)), pastelWrapList(array_fill(0, 1, 0)), pastelWrapList(array_fill(0, 1, 0)), pastelWrapList(array_fill(0, 1, 0)), pastelWrapList(array_fill(0, 2, 0)));
			$globals->commonStrings->arr[""] = $globals->stringEmpty;
			self::PST_assignIndexHack($globals->booleanType, 0, 2);
			self::PST_assignIndexHack($globals->intType, 0, 3);
			self::PST_assignIndexHack($globals->floatType, 0, 4);
			self::PST_assignIndexHack($globals->stringType, 0, 5);
			self::PST_assignIndexHack($globals->classType, 0, 10);
			self::PST_assignIndexHack($globals->anyInstanceType, 0, 8);
			self::PST_assignIndexHack($globals->anyInstanceType, 1, 0);
			return $globals;
		}

		public static function initializeFunction($vm, $args, $currentPc, $stringArg) {
			$functionId = $args->arr[0];
			$nameId = $args->arr[1];
			$minArgCount = $args->arr[2];
			$maxArgCount = $args->arr[3];
			$functionType = $args->arr[4];
			$classId = $args->arr[5];
			$localsCount = $args->arr[6];
			$numPcOffsetsForOptionalArgs = $args->arr[8];
			$pcOffsetsForOptionalArgs = pastelWrapList(array_fill(0, ($numPcOffsetsForOptionalArgs + 1), 0));
			$i = 0;
			while (($i < $numPcOffsetsForOptionalArgs)) {
				$pcOffsetsForOptionalArgs->arr[($i + 1)] = $args->arr[(9 + $i)];
				$i += 1;
			}
			$functionTable = self::getFunctionTable($vm, $functionId);
			$functionTable->arr[$functionId] = new FunctionInfo($functionId, $nameId, $currentPc, $minArgCount, $maxArgCount, $functionType, $classId, $localsCount, $pcOffsetsForOptionalArgs, $stringArg, null);
			$vm->metadata->mostRecentFunctionDef = $functionTable->arr[$functionId];
			if (($nameId >= 0)) {
				$name = $vm->metadata->identifiers->arr[$nameId];
				if ("_LIB_CORE_list_filter" === $name) {
					self::PST_assignIndexHack($vm->metadata->primitiveMethodFunctionIdFallbackLookup, 0, $functionId);
				} else if ("_LIB_CORE_list_map" === $name) {
					self::PST_assignIndexHack($vm->metadata->primitiveMethodFunctionIdFallbackLookup, 1, $functionId);
				} else if ("_LIB_CORE_list_sort_by_key" === $name) {
					self::PST_assignIndexHack($vm->metadata->primitiveMethodFunctionIdFallbackLookup, 2, $functionId);
				} else if ("_LIB_CORE_invoke" === $name) {
					self::PST_assignIndexHack($vm->metadata->primitiveMethodFunctionIdFallbackLookup, 3, $functionId);
				} else if ("_LIB_CORE_generateException" === $name) {
					$mn = $vm->metadata->magicNumbers;
					$mn->coreGenerateExceptionFunctionId = $functionId;
				}
			}
			return 0;
		}

		public static function initializeIntSwitchStatement($vm, $pc, $args) {
			$output = new PastelPtrArray();
			$i = 1;
			while (($i < count($args->arr))) {
				$output->arr['i'.$args->arr[$i]] = $args->arr[($i + 1)];
				$i += 2;
			}
			self::PST_assignIndexHack($vm->byteCode->integerSwitchesByPc, $pc, $output);
			return $output;
		}

		public static function initializeStringSwitchStatement($vm, $pc, $args) {
			$output = new PastelPtrArray();
			$i = 1;
			while (($i < count($args->arr))) {
				$s = $vm->metadata->literalTable->arr[$args->arr[$i]]->internalValue;
				$output->arr[$s] = $args->arr[($i + 1)];
				$i += 2;
			}
			self::PST_assignIndexHack($vm->byteCode->stringSwitchesByPc, $pc, $output);
			return $output;
		}

		public static function initLocTable($vm, $row) {
			$classId = $row->arr[0];
			$memberCount = $row->arr[1];
			$nameId = 0;
			$totalLocales = $vm->metadata->magicNumbers->totalLocaleCount;
			$lookup = new PastelPtrArray();
			$i = 2;
			while (($i < count($row->arr))) {
				$localeId = $row->arr[$i];
				$i += 1;
				$j = 0;
				while (($j < $memberCount)) {
					$nameId = $row->arr[($i + $j)];
					if (($nameId != -1)) {
						$lookup->arr['i'.(($nameId * $totalLocales) + $localeId)] = $j;
					}
					$j += 1;
				}
				$i += $memberCount;
			}
			$vm->metadata->classMemberLocalizerBuilder->arr['i'.$classId] = $lookup;
			return 0;
		}

		public static function interpret($vm, $executionContextId) {
			$output = self::interpretImpl($vm, $executionContextId);
			while ((($output->status == 5) && ($output->reinvokeDelay == 0))) {
				$output = self::interpretImpl($vm, $executionContextId);
			}
			return $output;
		}

		public static function interpreterFinished($vm, $ec) {
			if (($ec != null)) {
				$id = $ec->id;
				if (isset($vm->executionContexts->arr['i'.$id])) {
					unset($vm->executionContexts->arr['i'.$id]);
				}
			}
			return new InterpreterResult(1, null, 0.0, 0, false, "");
		}

		public static function interpreterGetExecutionContext($vm, $executionContextId) {
			$executionContexts = $vm->executionContexts;
			if (!isset($executionContexts->arr['i'.$executionContextId])) {
				return null;
			}
			return $executionContexts->arr['i'.$executionContextId];
		}

		public static function interpretImpl($vm, $executionContextId) {
			$metadata = $vm->metadata;
			$globals = $vm->globals;
			$VALUE_NULL = $globals->valueNull;
			$VALUE_TRUE = $globals->boolTrue;
			$VALUE_FALSE = $globals->boolFalse;
			$VALUE_INT_ONE = $globals->intOne;
			$VALUE_INT_ZERO = $globals->intZero;
			$VALUE_FLOAT_ZERO = $globals->floatZero;
			$VALUE_FLOAT_ONE = $globals->floatOne;
			$INTEGER_POSITIVE_CACHE = $globals->positiveIntegers;
			$INTEGER_NEGATIVE_CACHE = $globals->negativeIntegers;
			$executionContexts = $vm->executionContexts;
			$ec = self::interpreterGetExecutionContext($vm, $executionContextId);
			if (($ec == null)) {
				return self::interpreterFinished($vm, null);
			}
			$ec->executionCounter += 1;
			$stack = $ec->stackTop;
			$ops = $vm->byteCode->ops;
			$args = $vm->byteCode->args;
			$stringArgs = $vm->byteCode->stringArgs;
			$classTable = $vm->metadata->classTable;
			$functionTable = $vm->metadata->functionTable;
			$literalTable = $vm->metadata->literalTable;
			$identifiers = $vm->metadata->identifiers;
			$valueStack = $ec->valueStack;
			$valueStackSize = $ec->currentValueStackSize;
			$valueStackCapacity = count($valueStack->arr);
			$hasInterrupt = false;
			$type = 0;
			$nameId = 0;
			$classId = 0;
			$functionId = 0;
			$localeId = 0;
			$classInfo = null;
			$_len = 0;
			$root = null;
			$row = null;
			$argCount = 0;
			$stringList = null;
			$returnValueUsed = false;
			$output = null;
			$functionInfo = null;
			$keyType = 0;
			$intKey = 0;
			$stringKey = null;
			$first = false;
			$primitiveMethodToCoreLibraryFallback = false;
			$bool1 = false;
			$bool2 = false;
			$staticConstructorNotInvoked = true;
			$int1 = 0;
			$int2 = 0;
			$int3 = 0;
			$i = 0;
			$j = 0;
			$float1 = 0.0;
			$float2 = 0.0;
			$float3 = 0.0;
			$floatList1 = pastelWrapList(array_fill(0, 2, 0.0));
			$value = null;
			$value2 = null;
			$value3 = null;
			$string1 = null;
			$string2 = null;
			$objInstance1 = null;
			$objInstance2 = null;
			$list1 = null;
			$list2 = null;
			$valueList1 = null;
			$valueList2 = null;
			$dictImpl = null;
			$dictImpl2 = null;
			$stringList1 = null;
			$intList1 = null;
			$valueArray1 = null;
			$intArray1 = null;
			$intArray2 = null;
			$objArray1 = null;
			$functionPointer1 = null;
			$intIntDict1 = null;
			$stringIntDict1 = null;
			$stackFrame2 = null;
			$leftValue = null;
			$rightValue = null;
			$classValue = null;
			$arg1 = null;
			$arg2 = null;
			$arg3 = null;
			$tokenList = null;
			$globalNameIdToPrimitiveMethodName = $vm->metadata->globalNameIdToPrimitiveMethodName;
			$magicNumbers = $vm->metadata->magicNumbers;
			$integerSwitchesByPc = $vm->byteCode->integerSwitchesByPc;
			$stringSwitchesByPc = $vm->byteCode->stringSwitchesByPc;
			$integerSwitch = null;
			$stringSwitch = null;
			$esfData = $vm->metadata->esfData;
			$closure = null;
			$parentClosure = null;
			$intBuffer = pastelWrapList(array_fill(0, 16, 0));
			$localsStack = $ec->localsStack;
			$localsStackSet = $ec->localsStackSet;
			$localsStackSetToken = $stack->localsStackSetToken;
			$localsStackCapacity = count($localsStack->arr);
			$localsStackOffset = $stack->localsStackOffset;
			$funcArgs = $vm->funcArgs;
			$pc = $stack->pc;
			$nativeFp = null;
			$debugData = $vm->byteCode->debugData;
			$isBreakPointPresent = $debugData->hasBreakpoint;
			$breakpointInfo = null;
			$debugBreakPointTemporaryDisable = false;
			while (true) {
				$row = $args->arr[$pc];
				switch ($ops->arr[$pc]) {
					case 0:
						// ADD_LITERAL;
						self::addLiteralImpl($vm, $row, $stringArgs->arr[$pc]);
						break;
					case 1:
						// ADD_NAME;
						self::addNameImpl($vm, $stringArgs->arr[$pc]);
						break;
					case 2:
						// ARG_TYPE_VERIFY;
						$_len = $row->arr[0];
						$i = 1;
						$j = 0;
						while (($j < $_len)) {
							$j += 1;
						}
						break;
					case 3:
						// ASSIGN_CLOSURE;
						$value = $valueStack->arr[--$valueStackSize];
						$i = $row->arr[0];
						if (($stack->closureVariables == null)) {
							$closure = new PastelPtrArray();
							$closure->arr['i-1'] = new ClosureValuePointer($stack->objectContext);
							$stack->closureVariables = $closure;
							$closure->arr['i'.$i] = new ClosureValuePointer($value);
						} else {
							$closure = $stack->closureVariables;
							if (isset($closure->arr['i'.$i])) {
								$closure->arr['i'.$i]->value = $value;
							} else {
								$closure->arr['i'.$i] = new ClosureValuePointer($value);
							}
						}
						break;
					case 4:
						// ASSIGN_INDEX;
						$valueStackSize -= 3;
						$value = $valueStack->arr[($valueStackSize + 2)];
						$value2 = $valueStack->arr[($valueStackSize + 1)];
						$root = $valueStack->arr[$valueStackSize];
						$type = $root->type;
						$bool1 = ($row->arr[0] == 1);
						if (($type == 6)) {
							if (($value2->type == 3)) {
								$i = $value2->internalValue;
								$list1 = $root->internalValue;
								if (($list1->type != null)) {
									$value3 = self::canAssignTypeToGeneric($vm, $value, $list1->type, 0);
									if (($value3 == null)) {
										$hasInterrupt = self::EX_InvalidArgument($ec, implode(array("Cannot convert a ", self::typeToStringFromValue($vm, $value), " into a ", self::typeToString($vm, $list1->type, 0))));
									}
									$value = $value3;
								}
								if (!$hasInterrupt) {
									if (($i >= $list1->size)) {
										$hasInterrupt = self::EX_IndexOutOfRange($ec, "Index is out of range.");
									} else if (($i < 0)) {
										$i += $list1->size;
										if (($i < 0)) {
											$hasInterrupt = self::EX_IndexOutOfRange($ec, "Index is out of range.");
										}
									}
									if (!$hasInterrupt) {
										self::PST_assignIndexHack($list1->list, $i, $value);
									}
								}
							} else {
								$hasInterrupt = self::EX_InvalidArgument($ec, "List index must be an integer.");
							}
						} else if (($type == 7)) {
							$dictImpl = $root->internalValue;
							if (($dictImpl->valueType != null)) {
								$value3 = self::canAssignTypeToGeneric($vm, $value, $dictImpl->valueType, 0);
								if (($value3 == null)) {
									$hasInterrupt = self::EX_InvalidArgument($ec, "Cannot assign a value to this dictionary of this type.");
								} else {
									$value = $value3;
								}
							}
							$keyType = $value2->type;
							if (($keyType == 3)) {
								$intKey = $value2->internalValue;
							} else if (($keyType == 5)) {
								$stringKey = $value2->internalValue;
							} else if (($keyType == 8)) {
								$objInstance1 = $value2->internalValue;
								$intKey = $objInstance1->objectId;
							} else {
								$hasInterrupt = self::EX_InvalidArgument($ec, "Invalid key for a dictionary.");
							}
							if (!$hasInterrupt) {
								$bool2 = ($dictImpl->size == 0);
								if (($dictImpl->keyType != $keyType)) {
									if (($dictImpl->valueType != null)) {
										$string1 = implode(array("Cannot assign a key of type ", self::typeToStringFromValue($vm, $value2), " to a dictionary that requires key types of ", self::dictKeyInfoToString($vm, $dictImpl), "."));
										$hasInterrupt = self::EX_InvalidKey($ec, $string1);
									} else if (!$bool2) {
										$hasInterrupt = self::EX_InvalidKey($ec, "Cannot have multiple keys in one dictionary with different types.");
									}
								} else if ((($keyType == 8) && ($dictImpl->keyClassId > 0) && ($objInstance1->classId != $dictImpl->keyClassId))) {
									if (self::isClassASubclassOf($vm, $objInstance1->classId, $dictImpl->keyClassId)) {
										$hasInterrupt = self::EX_InvalidKey($ec, "Cannot use this type of object as a key for this dictionary.");
									}
								}
							}
							if (!$hasInterrupt) {
								if (($keyType == 5)) {
									$int1 = isset($dictImpl->stringToIndex->arr[$stringKey]) ? $dictImpl->stringToIndex->arr[$stringKey] : (-1);
									if (($int1 == -1)) {
										$dictImpl->stringToIndex->arr[$stringKey] = $dictImpl->size;
										$dictImpl->size += 1;
										array_push($dictImpl->keys->arr, $value2);
										array_push($dictImpl->values->arr, $value);
										if ($bool2) {
											$dictImpl->keyType = $keyType;
										}
									} else {
										self::PST_assignIndexHack($dictImpl->values, $int1, $value);
									}
								} else {
									$int1 = isset($dictImpl->intToIndex->arr['i'.$intKey]) ? $dictImpl->intToIndex->arr['i'.$intKey] : (-1);
									if (($int1 == -1)) {
										$dictImpl->intToIndex->arr['i'.$intKey] = $dictImpl->size;
										$dictImpl->size += 1;
										array_push($dictImpl->keys->arr, $value2);
										array_push($dictImpl->values->arr, $value);
										if ($bool2) {
											$dictImpl->keyType = $keyType;
										}
									} else {
										self::PST_assignIndexHack($dictImpl->values, $int1, $value);
									}
								}
							}
						} else {
							$hasInterrupt = self::EX_UnsupportedOperation($ec, ((self::getTypeFromId($type)) . (" type does not support assigning to an index.")));
						}
						if ($bool1) {
							$valueStack->arr[$valueStackSize] = $value;
							$valueStackSize += 1;
						}
						break;
					case 6:
						// ASSIGN_STATIC_FIELD;
						$classInfo = $classTable->arr[$row->arr[0]];
						$staticConstructorNotInvoked = true;
						if (($classInfo->staticInitializationState < 2)) {
							$stack->pc = $pc;
							$stackFrame2 = self::maybeInvokeStaticConstructor($vm, $ec, $stack, $classInfo, $valueStackSize, (self::PST_intBuffer16));
							if (((self::PST_intBuffer16)->arr[0] == 1)) {
								return self::generateException($vm, $stack, $pc, $valueStackSize, $ec, 0, "Static initialization loop detected. The class this field is a member of is not done being initialized.");
							}
							if (($stackFrame2 != null)) {
								$staticConstructorNotInvoked = false;
								$stack = $stackFrame2;
								$pc = $stack->pc;
								$localsStackSetToken = $stack->localsStackSetToken;
								$localsStackOffset = $stack->localsStackOffset;
							}
						}
						if ($staticConstructorNotInvoked) {
							$valueStackSize -= 1;
							self::PST_assignIndexHack($classInfo->staticFields, $row->arr[1], $valueStack->arr[$valueStackSize]);
						}
						break;
					case 7:
						// ASSIGN_FIELD;
						$valueStackSize -= 2;
						$value = $valueStack->arr[($valueStackSize + 1)];
						$value2 = $valueStack->arr[$valueStackSize];
						$nameId = $row->arr[2];
						if (($value2->type == 8)) {
							$objInstance1 = $value2->internalValue;
							$classId = $objInstance1->classId;
							$classInfo = $classTable->arr[$classId];
							$intIntDict1 = $classInfo->localeScopedNameIdToMemberId;
							if (($row->arr[5] == $classId)) {
								$int1 = $row->arr[6];
							} else {
								$int1 = isset($intIntDict1->arr['i'.$nameId]) ? $intIntDict1->arr['i'.$nameId] : (-1);
								if (($int1 != -1)) {
									$int3 = $classInfo->fieldAccessModifiers->arr[$int1];
									if (($int3 > 1)) {
										if (($int3 == 2)) {
											if (($classId != $row->arr[3])) {
												$int1 = -2;
											}
										} else {
											if ((($int3 == 3) || ($int3 == 5))) {
												if (($classInfo->assemblyId != $row->arr[4])) {
													$int1 = -3;
												}
											}
											if ((($int3 == 4) || ($int3 == 5))) {
												$i = $row->arr[3];
												if (($classId == $i)) {
												} else {
													$classInfo = $classTable->arr[$classInfo->id];
													while ((($classInfo->baseClassId != -1) && ($int1 < count($classTable->arr[$classInfo->baseClassId]->fieldAccessModifiers->arr)))) {
														$classInfo = $classTable->arr[$classInfo->baseClassId];
													}
													$j = $classInfo->id;
													if (($j != $i)) {
														$bool1 = false;
														while ((($i != -1) && ($classTable->arr[$i]->baseClassId != -1))) {
															$i = $classTable->arr[$i]->baseClassId;
															if (($i == $j)) {
																$bool1 = true;
																$i = -1;
															}
														}
														if (!$bool1) {
															$int1 = -4;
														}
													}
												}
												$classInfo = $classTable->arr[$classId];
											}
										}
									}
								}
								$row->arr[5] = $classId;
								$row->arr[6] = $int1;
							}
							if (($int1 > -1)) {
								$int2 = $classInfo->functionIds->arr[$int1];
								if (($int2 == -1)) {
									$intArray1 = $classInfo->typeInfo->arr[$int1];
									if (($intArray1 == null)) {
										self::PST_assignIndexHack($objInstance1->members, $int1, $value);
									} else {
										$value2 = self::canAssignTypeToGeneric($vm, $value, $intArray1, 0);
										if (($value2 != null)) {
											self::PST_assignIndexHack($objInstance1->members, $int1, $value2);
										} else {
											$hasInterrupt = self::EX_InvalidArgument($ec, "Cannot assign this type to this field.");
										}
									}
								} else {
									$hasInterrupt = self::EX_InvalidArgument($ec, "Cannot override a method with assignment.");
								}
							} else if (($int1 < -1)) {
								$string1 = $identifiers->arr[$row->arr[0]];
								if (($int1 == -2)) {
									$string2 = "private";
								} else if (($int1 == -3)) {
									$string2 = "internal";
								} else {
									$string2 = "protected";
								}
								$hasInterrupt = self::EX_UnknownField($ec, implode(array("The field '", $string1, "' is marked as ", $string2, " and cannot be accessed from here.")));
							} else {
								$hasInterrupt = self::EX_InvalidAssignment($ec, implode(array("'", $classInfo->fullyQualifiedName, "' instances do not have a field called '", $metadata->identifiers->arr[$row->arr[0]], "'")));
							}
						} else if (($value2->type == 1)) {
							$hasInterrupt = self::EX_NullReference($ec, "Cannot assign to a field on null.");
						} else {
							$hasInterrupt = self::EX_InvalidAssignment($ec, "Cannot assign to a field on this type.");
						}
						if (($row->arr[1] == 1)) {
							$valueStack->arr[$valueStackSize++] = $value;
						}
						break;
					case 8:
						// ASSIGN_THIS_FIELD;
						$objInstance2 = $stack->objectContext->internalValue;
						self::PST_assignIndexHack($objInstance2->members, $row->arr[0], $valueStack->arr[--$valueStackSize]);
						break;
					case 5:
						// ASSIGN_LOCAL;
						$i = ($localsStackOffset + $row->arr[0]);
						$localsStack->arr[$i] = $valueStack->arr[--$valueStackSize];
						$localsStackSet->arr[$i] = $localsStackSetToken;
						break;
					case 9:
						// BINARY_OP;
						$rightValue = $valueStack->arr[--$valueStackSize];
						$leftValue = $valueStack->arr[($valueStackSize - 1)];
						switch ((((($leftValue->type * 15) + $row->arr[0]) * 11) + $rightValue->type)) {
							case 553:
								// int ** int;
								$value = self::doExponentMath($globals, (0.0 + $leftValue->internalValue), (0.0 + $rightValue->internalValue), false);
								if (($value == null)) {
									$hasInterrupt = self::EX_InvalidArgument($ec, self::getExponentErrorMsg($vm, $leftValue, $rightValue));
								}
								break;
							case 554:
								// int ** float;
								$value = self::doExponentMath($globals, (0.0 + $leftValue->internalValue), $rightValue->internalValue, false);
								if (($value == null)) {
									$hasInterrupt = self::EX_InvalidArgument($ec, self::getExponentErrorMsg($vm, $leftValue, $rightValue));
								}
								break;
							case 718:
								// float ** int;
								$value = self::doExponentMath($globals, $leftValue->internalValue, (0.0 + $rightValue->internalValue), false);
								if (($value == null)) {
									$hasInterrupt = self::EX_InvalidArgument($ec, self::getExponentErrorMsg($vm, $leftValue, $rightValue));
								}
								break;
							case 719:
								// float ** float;
								$value = self::doExponentMath($globals, $leftValue->internalValue, $rightValue->internalValue, false);
								if (($value == null)) {
									$hasInterrupt = self::EX_InvalidArgument($ec, self::getExponentErrorMsg($vm, $leftValue, $rightValue));
								}
								break;
							case 708:
								// float % float;
								$float1 = $rightValue->internalValue;
								if (($float1 == 0)) {
									$hasInterrupt = self::EX_DivisionByZero($ec, "Modulo by 0.");
								} else {
									$float3 = ($leftValue->internalValue % $float1);
									if (($float3 < 0)) {
										$float3 += $float1;
									}
									$value = self::buildFloat($globals, $float3);
								}
								break;
							case 707:
								// float % int;
								$int1 = $rightValue->internalValue;
								if (($int1 == 0)) {
									$hasInterrupt = self::EX_DivisionByZero($ec, "Modulo by 0.");
								} else {
									$float1 = ($leftValue->internalValue % $int1);
									if (($float1 < 0)) {
										$float1 += $int1;
									}
									$value = self::buildFloat($globals, $float1);
								}
								break;
							case 543:
								// int % float;
								$float3 = $rightValue->internalValue;
								if (($float3 == 0)) {
									$hasInterrupt = self::EX_DivisionByZero($ec, "Modulo by 0.");
								} else {
									$float1 = ($leftValue->internalValue % $float3);
									if (($float1 < 0)) {
										$float1 += $float3;
									}
									$value = self::buildFloat($globals, $float1);
								}
								break;
							case 542:
								// int % int;
								$int2 = $rightValue->internalValue;
								if (($int2 == 0)) {
									$hasInterrupt = self::EX_DivisionByZero($ec, "Modulo by 0.");
								} else {
									$int1 = ($leftValue->internalValue % $int2);
									if (($int1 < 0)) {
										$int1 += $int2;
									}
									$value = self::buildInteger($globals, $int1);
								}
								break;
							case 996:
								// list + list;
								$value = new Value(6, self::valueConcatLists($leftValue->internalValue, $rightValue->internalValue));
								break;
							case 498:
								// int + int;
								$int1 = ($leftValue->internalValue + $rightValue->internalValue);
								if (($int1 < 0)) {
									if (($int1 > -257)) {
										$value = $INTEGER_NEGATIVE_CACHE->arr[-$int1];
									} else {
										$value = new Value(3, $int1);
									}
								} else if (($int1 < 2049)) {
									$value = $INTEGER_POSITIVE_CACHE->arr[$int1];
								} else {
									$value = new Value(3, $int1);
								}
								break;
							case 509:
								// int - int;
								$int1 = ($leftValue->internalValue - $rightValue->internalValue);
								if (($int1 < 0)) {
									if (($int1 > -257)) {
										$value = $INTEGER_NEGATIVE_CACHE->arr[-$int1];
									} else {
										$value = new Value(3, $int1);
									}
								} else if (($int1 < 2049)) {
									$value = $INTEGER_POSITIVE_CACHE->arr[$int1];
								} else {
									$value = new Value(3, $int1);
								}
								break;
							case 520:
								// int * int;
								$int1 = ($leftValue->internalValue * $rightValue->internalValue);
								if (($int1 < 0)) {
									if (($int1 > -257)) {
										$value = $INTEGER_NEGATIVE_CACHE->arr[-$int1];
									} else {
										$value = new Value(3, $int1);
									}
								} else if (($int1 < 2049)) {
									$value = $INTEGER_POSITIVE_CACHE->arr[$int1];
								} else {
									$value = new Value(3, $int1);
								}
								break;
							case 531:
								// int / int;
								$int1 = $leftValue->internalValue;
								$int2 = $rightValue->internalValue;
								if (($int2 == 0)) {
									$hasInterrupt = self::EX_DivisionByZero($ec, "Division by 0.");
								} else if (($int1 == 0)) {
									$value = $VALUE_INT_ZERO;
								} else {
									if ((($int1 % $int2) == 0)) {
										$int3 = intval(($int1) / ($int2));
									} else if (((($int1 < 0)) != (($int2 < 0)))) {
										$float1 = (1 + (((-1.0 * $int1)) / ($int2)));
										$float1 -= ($float1 % 1.0);
										$int3 = intval((-$float1));
									} else {
										$int3 = intval(($int1) / ($int2));
									}
									if (($int3 < 0)) {
										if (($int3 > -257)) {
											$value = $INTEGER_NEGATIVE_CACHE->arr[-$int3];
										} else {
											$value = new Value(3, $int3);
										}
									} else if (($int3 < 2049)) {
										$value = $INTEGER_POSITIVE_CACHE->arr[$int3];
									} else {
										$value = new Value(3, $int3);
									}
								}
								break;
							case 663:
								// float + int;
								$value = self::buildFloat($globals, ($leftValue->internalValue + $rightValue->internalValue));
								break;
							case 499:
								// int + float;
								$value = self::buildFloat($globals, ($leftValue->internalValue + $rightValue->internalValue));
								break;
							case 664:
								// float + float;
								$float1 = ($leftValue->internalValue + $rightValue->internalValue);
								if (($float1 == 0)) {
									$value = $VALUE_FLOAT_ZERO;
								} else if (($float1 == 1)) {
									$value = $VALUE_FLOAT_ONE;
								} else {
									$value = new Value(4, $float1);
								}
								break;
							case 510:
								// int - float;
								$value = self::buildFloat($globals, ($leftValue->internalValue - $rightValue->internalValue));
								break;
							case 674:
								// float - int;
								$value = self::buildFloat($globals, ($leftValue->internalValue - $rightValue->internalValue));
								break;
							case 675:
								// float - float;
								$float1 = ($leftValue->internalValue - $rightValue->internalValue);
								if (($float1 == 0)) {
									$value = $VALUE_FLOAT_ZERO;
								} else if (($float1 == 1)) {
									$value = $VALUE_FLOAT_ONE;
								} else {
									$value = new Value(4, $float1);
								}
								break;
							case 685:
								// float * int;
								$value = self::buildFloat($globals, ($leftValue->internalValue * $rightValue->internalValue));
								break;
							case 521:
								// int * float;
								$value = self::buildFloat($globals, ($leftValue->internalValue * $rightValue->internalValue));
								break;
							case 686:
								// float * float;
								$value = self::buildFloat($globals, ($leftValue->internalValue * $rightValue->internalValue));
								break;
							case 532:
								// int / float;
								$float1 = $rightValue->internalValue;
								if (($float1 == 0)) {
									$hasInterrupt = self::EX_DivisionByZero($ec, "Division by 0.");
								} else {
									$value = self::buildFloat($globals, (($leftValue->internalValue) / ($float1)));
								}
								break;
							case 696:
								// float / int;
								$int1 = $rightValue->internalValue;
								if (($int1 == 0)) {
									$hasInterrupt = self::EX_DivisionByZero($ec, "Division by 0.");
								} else {
									$value = self::buildFloat($globals, (($leftValue->internalValue) / ($int1)));
								}
								break;
							case 697:
								// float / float;
								$float1 = $rightValue->internalValue;
								if (($float1 == 0)) {
									$hasInterrupt = self::EX_DivisionByZero($ec, "Division by 0.");
								} else {
									$value = self::buildFloat($globals, (($leftValue->internalValue) / ($float1)));
								}
								break;
							case 564:
								// int & int;
								$value = self::buildInteger($globals, ($leftValue->internalValue & $rightValue->internalValue));
								break;
							case 575:
								// int | int;
								$value = self::buildInteger($globals, ($leftValue->internalValue | $rightValue->internalValue));
								break;
							case 586:
								// int ^ int;
								$value = self::buildInteger($globals, ($leftValue->internalValue ^ $rightValue->internalValue));
								break;
							case 597:
								// int << int;
								$int1 = $rightValue->internalValue;
								if (($int1 < 0)) {
									$hasInterrupt = self::EX_InvalidArgument($ec, "Cannot bit shift by a negative number.");
								} else {
									$value = self::buildInteger($globals, ($leftValue->internalValue << $int1));
								}
								break;
							case 608:
								// int >> int;
								$int1 = $rightValue->internalValue;
								if (($int1 < 0)) {
									$hasInterrupt = self::EX_InvalidArgument($ec, "Cannot bit shift by a negative number.");
								} else {
									$value = self::buildInteger($globals, ($leftValue->internalValue >> $int1));
								}
								break;
							case 619:
								// int < int;
								if (($leftValue->internalValue < $rightValue->internalValue)) {
									$value = $VALUE_TRUE;
								} else {
									$value = $VALUE_FALSE;
								}
								break;
							case 630:
								// int <= int;
								if (($leftValue->internalValue <= $rightValue->internalValue)) {
									$value = $VALUE_TRUE;
								} else {
									$value = $VALUE_FALSE;
								}
								break;
							case 784:
								// float < int;
								if (($leftValue->internalValue < $rightValue->internalValue)) {
									$value = $VALUE_TRUE;
								} else {
									$value = $VALUE_FALSE;
								}
								break;
							case 795:
								// float <= int;
								if (($leftValue->internalValue <= $rightValue->internalValue)) {
									$value = $VALUE_TRUE;
								} else {
									$value = $VALUE_FALSE;
								}
								break;
							case 620:
								// int < float;
								if (($leftValue->internalValue < $rightValue->internalValue)) {
									$value = $VALUE_TRUE;
								} else {
									$value = $VALUE_FALSE;
								}
								break;
							case 631:
								// int <= float;
								if (($leftValue->internalValue <= $rightValue->internalValue)) {
									$value = $VALUE_TRUE;
								} else {
									$value = $VALUE_FALSE;
								}
								break;
							case 785:
								// float < float;
								if (($leftValue->internalValue < $rightValue->internalValue)) {
									$value = $VALUE_TRUE;
								} else {
									$value = $VALUE_FALSE;
								}
								break;
							case 796:
								// float <= float;
								if (($leftValue->internalValue <= $rightValue->internalValue)) {
									$value = $VALUE_TRUE;
								} else {
									$value = $VALUE_FALSE;
								}
								break;
							case 652:
								// int >= int;
								if (($leftValue->internalValue >= $rightValue->internalValue)) {
									$value = $VALUE_TRUE;
								} else {
									$value = $VALUE_FALSE;
								}
								break;
							case 641:
								// int > int;
								if (($leftValue->internalValue > $rightValue->internalValue)) {
									$value = $VALUE_TRUE;
								} else {
									$value = $VALUE_FALSE;
								}
								break;
							case 817:
								// float >= int;
								if (($leftValue->internalValue >= $rightValue->internalValue)) {
									$value = $VALUE_TRUE;
								} else {
									$value = $VALUE_FALSE;
								}
								break;
							case 806:
								// float > int;
								if (($leftValue->internalValue > $rightValue->internalValue)) {
									$value = $VALUE_TRUE;
								} else {
									$value = $VALUE_FALSE;
								}
								break;
							case 653:
								// int >= float;
								if (($leftValue->internalValue >= $rightValue->internalValue)) {
									$value = $VALUE_TRUE;
								} else {
									$value = $VALUE_FALSE;
								}
								break;
							case 642:
								// int > float;
								if (($leftValue->internalValue > $rightValue->internalValue)) {
									$value = $VALUE_TRUE;
								} else {
									$value = $VALUE_FALSE;
								}
								break;
							case 818:
								// float >= float;
								if (($leftValue->internalValue >= $rightValue->internalValue)) {
									$value = $VALUE_TRUE;
								} else {
									$value = $VALUE_FALSE;
								}
								break;
							case 807:
								// float > float;
								if (($leftValue->internalValue > $rightValue->internalValue)) {
									$value = $VALUE_TRUE;
								} else {
									$value = $VALUE_FALSE;
								}
								break;
							case 830:
								// string + string;
								$value = new Value(5, (($leftValue->internalValue) . ($rightValue->internalValue)));
								break;
							case 850:
								// string * int;
								$value = self::multiplyString($globals, $leftValue, $leftValue->internalValue, $rightValue->internalValue);
								break;
							case 522:
								// int * string;
								$value = self::multiplyString($globals, $rightValue, $rightValue->internalValue, $leftValue->internalValue);
								break;
							case 1015:
								// list * int;
								$int1 = $rightValue->internalValue;
								if (($int1 < 0)) {
									$hasInterrupt = self::EX_UnsupportedOperation($ec, "Cannot multiply list by negative number.");
								} else {
									$value = new Value(6, self::valueMultiplyList($leftValue->internalValue, $int1));
								}
								break;
							case 523:
								// int * list;
								$int1 = $leftValue->internalValue;
								if (($int1 < 0)) {
									$hasInterrupt = self::EX_UnsupportedOperation($ec, "Cannot multiply list by negative number.");
								} else {
									$value = new Value(6, self::valueMultiplyList($rightValue->internalValue, $int1));
								}
								break;
							default:
								if ((($row->arr[0] == 0) && ((($leftValue->type == 5) || ($rightValue->type == 5))))) {
									$value = new Value(5, ((self::valueToString($vm, $leftValue)) . (self::valueToString($vm, $rightValue))));
								} else {
									// unrecognized op;
									$hasInterrupt = self::EX_UnsupportedOperation($ec, implode(array("The '", self::getBinaryOpFromId($row->arr[0]), "' operator is not supported for these types: ", self::getTypeFromId($leftValue->type), " and ", self::getTypeFromId($rightValue->type))));
								}
								break;
						}
						$valueStack->arr[($valueStackSize - 1)] = $value;
						break;
					case 10:
						// BOOLEAN_NOT;
						$value = $valueStack->arr[($valueStackSize - 1)];
						if (($value->type != 2)) {
							$hasInterrupt = self::EX_InvalidArgument($ec, "Boolean expected.");
						} else if ($value->internalValue) {
							$valueStack->arr[($valueStackSize - 1)] = $VALUE_FALSE;
						} else {
							$valueStack->arr[($valueStackSize - 1)] = $VALUE_TRUE;
						}
						break;
					case 11:
						// BREAK;
						if (($row->arr[0] == 1)) {
							$pc += $row->arr[1];
						} else {
							$intArray1 = $esfData->arr[$pc];
							$pc = ($intArray1->arr[1] - 1);
							$valueStackSize = $stack->valueStackPopSize;
							$stack->postFinallyBehavior = 1;
						}
						break;
					case 12:
						// CALL_FUNCTION;
						$type = $row->arr[0];
						$argCount = $row->arr[1];
						$functionId = $row->arr[2];
						$returnValueUsed = ($row->arr[3] == 1);
						$classId = $row->arr[4];
						if ((($type == 2) || ($type == 6))) {
							// constructor or static method;
							$classInfo = $metadata->classTable->arr[$classId];
							$staticConstructorNotInvoked = true;
							if (($classInfo->staticInitializationState < 2)) {
								$stack->pc = $pc;
								$stackFrame2 = self::maybeInvokeStaticConstructor($vm, $ec, $stack, $classInfo, $valueStackSize, (self::PST_intBuffer16));
								if (((self::PST_intBuffer16)->arr[0] == 1)) {
									return self::generateException($vm, $stack, $pc, $valueStackSize, $ec, 0, "Static initialization loop detected. The class this field is a member of is not done being initialized.");
								}
								if (($stackFrame2 != null)) {
									$staticConstructorNotInvoked = false;
									$stack = $stackFrame2;
									$pc = $stack->pc;
									$localsStackSetToken = $stack->localsStackSetToken;
									$localsStackOffset = $stack->localsStackOffset;
								}
							}
						} else {
							$staticConstructorNotInvoked = true;
						}
						if ($staticConstructorNotInvoked) {
							$bool1 = true;
							// construct args array;
							if (($argCount == -1)) {
								$valueStackSize -= 1;
								$value = $valueStack->arr[$valueStackSize];
								if (($value->type == 1)) {
									$argCount = 0;
								} else if (($value->type == 6)) {
									$list1 = $value->internalValue;
									$argCount = $list1->size;
									$i = ($argCount - 1);
									while (($i >= 0)) {
										$funcArgs->arr[$i] = $list1->list->arr[$i];
										$i -= 1;
									}
								} else {
									$hasInterrupt = self::EX_InvalidArgument($ec, "Function pointers' .invoke method requires a list argument.");
								}
							} else {
								$i = ($argCount - 1);
								while (($i >= 0)) {
									$valueStackSize -= 1;
									$funcArgs->arr[$i] = $valueStack->arr[$valueStackSize];
									$i -= 1;
								}
							}
							if (!$hasInterrupt) {
								if (($type == 3)) {
									$value = $stack->objectContext;
									$objInstance1 = $value->internalValue;
									if (($objInstance1->classId != $classId)) {
										$int2 = $row->arr[5];
										if (($int2 != -1)) {
											$classInfo = $classTable->arr[$objInstance1->classId];
											$functionId = $classInfo->functionIds->arr[$int2];
										}
									}
								} else if (($type == 5)) {
									// field invocation;
									$valueStackSize -= 1;
									$value = $valueStack->arr[$valueStackSize];
									$localeId = $row->arr[5];
									switch ($value->type) {
										case 1:
											$hasInterrupt = self::EX_NullReference($ec, "Invoked method on null.");
											break;
										case 8:
											// field invoked on an object instance.;
											$objInstance1 = $value->internalValue;
											$int1 = $objInstance1->classId;
											$classInfo = $classTable->arr[$int1];
											$intIntDict1 = $classInfo->localeScopedNameIdToMemberId;
											$int1 = (($row->arr[4] * $magicNumbers->totalLocaleCount) + $row->arr[5]);
											$i = isset($intIntDict1->arr['i'.$int1]) ? $intIntDict1->arr['i'.$int1] : (-1);
											if (($i != -1)) {
												$int1 = $intIntDict1->arr['i'.$int1];
												$functionId = $classInfo->functionIds->arr[$int1];
												if (($functionId > 0)) {
													$type = 3;
												} else {
													$value = $objInstance1->members->arr[$int1];
													$type = 4;
													$valueStack->arr[$valueStackSize] = $value;
													$valueStackSize += 1;
												}
											} else {
												$hasInterrupt = self::EX_UnknownField($ec, "Unknown field.");
											}
											break;
										case 10:
											// field invocation on a class object instance.;
											$functionId = self::resolvePrimitiveMethodName2($globalNameIdToPrimitiveMethodName, $value->type, $classId);
											if (($functionId < 0)) {
												$hasInterrupt = self::EX_InvalidInvocation($ec, "Class definitions do not have that method.");
											} else {
												$functionId = self::resolvePrimitiveMethodName2($globalNameIdToPrimitiveMethodName, $value->type, $classId);
												if (($functionId < 0)) {
													$hasInterrupt = self::EX_InvalidInvocation($ec, ((self::getTypeFromId($value->type)) . (" does not have that method.")));
												} else if (($globalNameIdToPrimitiveMethodName->arr[$classId] == 8)) {
													$type = 6;
													$classValue = $value->internalValue;
													if ($classValue->isInterface) {
														$hasInterrupt = self::EX_UnsupportedOperation($ec, "Cannot create an instance of an interface.");
													} else {
														$classId = $classValue->classId;
														if (!$returnValueUsed) {
															$hasInterrupt = self::EX_UnsupportedOperation($ec, "Cannot create an instance and not use the output.");
														} else {
															$classInfo = $metadata->classTable->arr[$classId];
															$functionId = $classInfo->constructorFunctionId;
														}
													}
												} else {
													$type = 9;
												}
											}
											break;
										default:
											// primitive method suspected.;
											$functionId = self::resolvePrimitiveMethodName2($globalNameIdToPrimitiveMethodName, $value->type, $classId);
											if (($functionId < 0)) {
												$hasInterrupt = self::EX_InvalidInvocation($ec, ((self::getTypeFromId($value->type)) . (" does not have that method.")));
											} else {
												$type = 9;
											}
											break;
									}
								}
							}
							if ((($type == 4) && !$hasInterrupt)) {
								// pointer provided;
								$valueStackSize -= 1;
								$value = $valueStack->arr[$valueStackSize];
								if (($value->type == 9)) {
									$functionPointer1 = $value->internalValue;
									switch ($functionPointer1->type) {
										case 1:
											// pointer to a function;
											$functionId = $functionPointer1->functionId;
											$type = 1;
											break;
										case 2:
											// pointer to a method;
											$functionId = $functionPointer1->functionId;
											$value = $functionPointer1->context;
											$type = 3;
											break;
										case 3:
											// pointer to a static method;
											$functionId = $functionPointer1->functionId;
											$classId = $functionPointer1->classId;
											$type = 2;
											break;
										case 4:
											// pointer to a primitive method;
											$value = $functionPointer1->context;
											$functionId = $functionPointer1->functionId;
											$type = 9;
											break;
										case 5:
											// lambda instance;
											$value = $functionPointer1->context;
											$functionId = $functionPointer1->functionId;
											$type = 10;
											$closure = $functionPointer1->closureVariables;
											break;
									}
								} else {
									$hasInterrupt = self::EX_InvalidInvocation($ec, "This type cannot be invoked like a function.");
								}
							}
							if ((($type == 9) && !$hasInterrupt)) {
								// primitive method invocation;
								$output = $VALUE_NULL;
								$primitiveMethodToCoreLibraryFallback = false;
								switch ($value->type) {
									case 5:
										// ...on a string;
										$string1 = $value->internalValue;
										switch ($functionId) {
											case 7:
												if (($argCount != 1)) {
													$hasInterrupt = self::EX_InvalidArgument($ec, self::primitiveMethodWrongArgCountError("string contains method", 1, $argCount));
												} else {
													$value2 = $funcArgs->arr[0];
													if (($value2->type != 5)) {
														$hasInterrupt = self::EX_InvalidArgument($ec, "string contains method requires another string as input.");
													} else if ((strpos($string1, $value2->internalValue) !== false)) {
														$output = $VALUE_TRUE;
													} else {
														$output = $VALUE_FALSE;
													}
												}
												break;
											case 9:
												if (($argCount != 1)) {
													$hasInterrupt = self::EX_InvalidArgument($ec, self::primitiveMethodWrongArgCountError("string endsWith method", 1, $argCount));
												} else {
													$value2 = $funcArgs->arr[0];
													if (($value2->type != 5)) {
														$hasInterrupt = self::EX_InvalidArgument($ec, "string endsWith method requires another string as input.");
													} else if (self::PST_stringEndsWith($string1, $value2->internalValue)) {
														$output = $VALUE_TRUE;
													} else {
														$output = $VALUE_FALSE;
													}
												}
												break;
											case 13:
												if ((($argCount < 1) || ($argCount > 2))) {
													$hasInterrupt = self::EX_InvalidArgument($ec, self::primitiveMethodWrongArgCountError("string indexOf method", 1, $argCount));
												} else {
													$value2 = $funcArgs->arr[0];
													if (($value2->type != 5)) {
														$hasInterrupt = self::EX_InvalidArgument($ec, "string indexOf method requires another string as input.");
													} else if (($argCount == 1)) {
														$output = self::buildInteger($globals, self::PST_stringIndexOf($string1, $value2->internalValue, 0));
													} else if (($funcArgs->arr[1]->type != 3)) {
														$hasInterrupt = self::EX_InvalidArgument($ec, "string indexOf method requires an integer as its second argument.");
													} else {
														$int1 = $funcArgs->arr[1]->internalValue;
														if ((($int1 < 0) || ($int1 >= strlen($string1)))) {
															$hasInterrupt = self::EX_IndexOutOfRange($ec, "String index is out of bounds.");
														} else {
															$output = self::buildInteger($globals, self::PST_stringIndexOf($string1, $value2->internalValue, $int1));
														}
													}
												}
												break;
											case 19:
												if (($argCount > 0)) {
													$hasInterrupt = self::EX_InvalidArgument($ec, self::primitiveMethodWrongArgCountError("string lower method", 0, $argCount));
												} else {
													$output = self::buildString($globals, strtolower($string1));
												}
												break;
											case 20:
												if (($argCount > 0)) {
													$hasInterrupt = self::EX_InvalidArgument($ec, self::primitiveMethodWrongArgCountError("string ltrim method", 0, $argCount));
												} else {
													$output = self::buildString($globals, ltrim($string1));
												}
												break;
											case 25:
												if (($argCount != 2)) {
													$hasInterrupt = self::EX_InvalidArgument($ec, self::primitiveMethodWrongArgCountError("string replace method", 2, $argCount));
												} else {
													$value2 = $funcArgs->arr[0];
													$value3 = $funcArgs->arr[1];
													if ((($value2->type != 5) || ($value3->type != 5))) {
														$hasInterrupt = self::EX_InvalidArgument($ec, "string replace method requires 2 strings as input.");
													} else {
														$output = self::buildString($globals, str_replace($value2->internalValue, $value3->internalValue, $string1));
													}
												}
												break;
											case 26:
												if (($argCount > 0)) {
													$hasInterrupt = self::EX_InvalidArgument($ec, self::primitiveMethodWrongArgCountError("string reverse method", 0, $argCount));
												} else {
													$output = self::buildString($globals, strrev($string1));
												}
												break;
											case 27:
												if (($argCount > 0)) {
													$hasInterrupt = self::EX_InvalidArgument($ec, self::primitiveMethodWrongArgCountError("string rtrim method", 0, $argCount));
												} else {
													$output = self::buildString($globals, rtrim($string1));
												}
												break;
											case 30:
												if (($argCount != 1)) {
													$hasInterrupt = self::EX_InvalidArgument($ec, self::primitiveMethodWrongArgCountError("string split method", 1, $argCount));
												} else {
													$value2 = $funcArgs->arr[0];
													if (($value2->type != 5)) {
														$hasInterrupt = self::EX_InvalidArgument($ec, "string split method requires another string as input.");
													} else {
														$stringList = pastelWrapList(explode($value2->internalValue, $string1));
														$_len = count($stringList->arr);
														$list1 = self::makeEmptyList($globals->stringType, $_len);
														$i = 0;
														while (($i < $_len)) {
															array_push($list1->list->arr, self::buildString($globals, $stringList->arr[$i]));
															$i += 1;
														}
														$list1->size = $_len;
														$output = new Value(6, $list1);
													}
												}
												break;
											case 31:
												if (($argCount != 1)) {
													$hasInterrupt = self::EX_InvalidArgument($ec, self::primitiveMethodWrongArgCountError("string startsWith method", 1, $argCount));
												} else {
													$value2 = $funcArgs->arr[0];
													if (($value2->type != 5)) {
														$hasInterrupt = self::EX_InvalidArgument($ec, "string startsWith method requires another string as input.");
													} else if (self::PST_stringStartsWith($string1, $value2->internalValue)) {
														$output = $VALUE_TRUE;
													} else {
														$output = $VALUE_FALSE;
													}
												}
												break;
											case 32:
												if (($argCount > 0)) {
													$hasInterrupt = self::EX_InvalidArgument($ec, self::primitiveMethodWrongArgCountError("string trim method", 0, $argCount));
												} else {
													$output = self::buildString($globals, trim($string1));
												}
												break;
											case 33:
												if (($argCount > 0)) {
													$hasInterrupt = self::EX_InvalidArgument($ec, self::primitiveMethodWrongArgCountError("string upper method", 0, $argCount));
												} else {
													$output = self::buildString($globals, strtoupper($string1));
												}
												break;
											default:
												$output = null;
												break;
										}
										break;
									case 6:
										// ...on a list;
										$list1 = $value->internalValue;
										switch ($functionId) {
											case 0:
												if (($argCount == 0)) {
													$hasInterrupt = self::EX_InvalidArgument($ec, "List add method requires at least one argument.");
												} else {
													$intArray1 = $list1->type;
													$i = 0;
													while (($i < $argCount)) {
														$value = $funcArgs->arr[$i];
														if (($intArray1 != null)) {
															$value2 = self::canAssignTypeToGeneric($vm, $value, $intArray1, 0);
															if (($value2 == null)) {
																$hasInterrupt = self::EX_InvalidArgument($ec, implode(array("Cannot convert a ", self::typeToStringFromValue($vm, $value), " into a ", self::typeToString($vm, $list1->type, 0))));
															}
															array_push($list1->list->arr, $value2);
														} else {
															array_push($list1->list->arr, $value);
														}
														$i += 1;
													}
													$list1->size += $argCount;
													$output = $VALUE_NULL;
												}
												break;
											case 3:
												if (($argCount > 0)) {
													$hasInterrupt = self::EX_InvalidArgument($ec, self::primitiveMethodWrongArgCountError("list choice method", 0, $argCount));
												} else {
													$_len = $list1->size;
													if (($_len == 0)) {
														$hasInterrupt = self::EX_UnsupportedOperation($ec, "Cannot use list.choice() method on an empty list.");
													} else {
														$i = intval((((random_int(0, PHP_INT_MAX - 1) / PHP_INT_MAX) * $_len)));
														$output = $list1->list->arr[$i];
													}
												}
												break;
											case 4:
												if (($argCount > 0)) {
													$hasInterrupt = self::EX_InvalidArgument($ec, self::primitiveMethodWrongArgCountError("list clear method", 0, $argCount));
												} else if (($list1->size > 0)) {
													$list1->list->arr = array();
													$list1->size = 0;
												}
												break;
											case 5:
												if (($argCount > 0)) {
													$hasInterrupt = self::EX_InvalidArgument($ec, self::primitiveMethodWrongArgCountError("list clone method", 0, $argCount));
												} else {
													$_len = $list1->size;
													$list2 = self::makeEmptyList($list1->type, $_len);
													$i = 0;
													while (($i < $_len)) {
														array_push($list2->list->arr, $list1->list->arr[$i]);
														$i += 1;
													}
													$list2->size = $_len;
													$output = new Value(6, $list2);
												}
												break;
											case 6:
												if (($argCount != 1)) {
													$hasInterrupt = self::EX_InvalidArgument($ec, self::primitiveMethodWrongArgCountError("list concat method", 1, $argCount));
												} else {
													$value2 = $funcArgs->arr[0];
													if (($value2->type != 6)) {
														$hasInterrupt = self::EX_InvalidArgument($ec, "list concat methods requires a list as an argument.");
													} else {
														$list2 = $value2->internalValue;
														$intArray1 = $list1->type;
														if ((($intArray1 != null) && !self::canAssignGenericToGeneric($vm, $list2->type, 0, $intArray1, 0, $intBuffer))) {
															$hasInterrupt = self::EX_InvalidArgument($ec, "Cannot concat a list: incompatible types.");
														} else {
															if ((($intArray1 != null) && ($intArray1->arr[0] == 4) && ($list2->type->arr[0] == 3))) {
																$bool1 = true;
															} else {
																$bool1 = false;
															}
															$_len = $list2->size;
															$i = 0;
															while (($i < $_len)) {
																$value = $list2->list->arr[$i];
																if ($bool1) {
																	$value = self::buildFloat($globals, (0.0 + $value->internalValue));
																}
																array_push($list1->list->arr, $value);
																$i += 1;
															}
															$list1->size += $_len;
														}
													}
												}
												break;
											case 7:
												if (($argCount != 1)) {
													$hasInterrupt = self::EX_InvalidArgument($ec, self::primitiveMethodWrongArgCountError("list contains method", 1, $argCount));
												} else {
													$value2 = $funcArgs->arr[0];
													$_len = $list1->size;
													$output = $VALUE_FALSE;
													$i = 0;
													while (($i < $_len)) {
														$value = $list1->list->arr[$i];
														if ((self::doEqualityComparisonAndReturnCode($value2, $value) == 1)) {
															$output = $VALUE_TRUE;
															$i = $_len;
														}
														$i += 1;
													}
												}
												break;
											case 10:
												if (($argCount != 1)) {
													$hasInterrupt = self::EX_InvalidArgument($ec, self::primitiveMethodWrongArgCountError("list filter method", 1, $argCount));
												} else {
													$value2 = $funcArgs->arr[0];
													if (($value2->type != 9)) {
														$hasInterrupt = self::EX_InvalidArgument($ec, "list filter method requires a function pointer as its argument.");
													} else {
														$primitiveMethodToCoreLibraryFallback = true;
														$functionId = $metadata->primitiveMethodFunctionIdFallbackLookup->arr[0];
														$funcArgs->arr[1] = $value;
														$argCount = 2;
														$output = null;
													}
												}
												break;
											case 14:
												if (($argCount != 2)) {
													$hasInterrupt = self::EX_InvalidArgument($ec, self::primitiveMethodWrongArgCountError("list insert method", 1, $argCount));
												} else {
													$value = $funcArgs->arr[0];
													$value2 = $funcArgs->arr[1];
													if (($value->type != 3)) {
														$hasInterrupt = self::EX_InvalidArgument($ec, "First argument of list.insert must be an integer index.");
													} else {
														$intArray1 = $list1->type;
														if (($intArray1 != null)) {
															$value3 = self::canAssignTypeToGeneric($vm, $value2, $intArray1, 0);
															if (($value3 == null)) {
																$hasInterrupt = self::EX_InvalidArgument($ec, "Cannot insert this type into this type of list.");
															}
															$value2 = $value3;
														}
														if (!$hasInterrupt) {
															$int1 = $value->internalValue;
															$_len = $list1->size;
															if (($int1 < 0)) {
																$int1 += $_len;
															}
															if (($int1 == $_len)) {
																array_push($list1->list->arr, $value2);
																$list1->size += 1;
															} else if ((($int1 < 0) || ($int1 >= $_len))) {
																$hasInterrupt = self::EX_IndexOutOfRange($ec, "Index out of range.");
															} else {
																array_splice($list1->list->arr, $int1, 0, array($value2));
																$list1->size += 1;
															}
														}
													}
												}
												break;
											case 17:
												if (($argCount != 1)) {
													if (($argCount == 0)) {
														$value2 = $globals->stringEmpty;
													} else {
														$hasInterrupt = self::EX_InvalidArgument($ec, self::primitiveMethodWrongArgCountError("list join method", 1, $argCount));
													}
												} else {
													$value2 = $funcArgs->arr[0];
													if (($value2->type != 5)) {
														$hasInterrupt = self::EX_InvalidArgument($ec, "Argument of list.join needs to be a string.");
													}
												}
												if (!$hasInterrupt) {
													$stringList1 = new PastelPtrArray();
													$string1 = $value2->internalValue;
													$_len = $list1->size;
													$i = 0;
													while (($i < $_len)) {
														$value = $list1->list->arr[$i];
														if (($value->type != 5)) {
															$string2 = self::valueToString($vm, $value);
														} else {
															$string2 = $value->internalValue;
														}
														array_push($stringList1->arr, $string2);
														$i += 1;
													}
													$output = self::buildString($globals, implode($string1, $stringList1->arr));
												}
												break;
											case 21:
												if (($argCount != 1)) {
													$hasInterrupt = self::EX_InvalidArgument($ec, self::primitiveMethodWrongArgCountError("list map method", 1, $argCount));
												} else {
													$value2 = $funcArgs->arr[0];
													if (($value2->type != 9)) {
														$hasInterrupt = self::EX_InvalidArgument($ec, "list map method requires a function pointer as its argument.");
													} else {
														$primitiveMethodToCoreLibraryFallback = true;
														$functionId = $metadata->primitiveMethodFunctionIdFallbackLookup->arr[1];
														$funcArgs->arr[1] = $value;
														$argCount = 2;
														$output = null;
													}
												}
												break;
											case 23:
												if (($argCount > 0)) {
													$hasInterrupt = self::EX_InvalidArgument($ec, self::primitiveMethodWrongArgCountError("list pop method", 0, $argCount));
												} else {
													$_len = $list1->size;
													if (($_len < 1)) {
														$hasInterrupt = self::EX_IndexOutOfRange($ec, "Cannot pop from an empty list.");
													} else {
														$_len -= 1;
														$value = array_pop($list1->list->arr);
														if ($returnValueUsed) {
															$output = $value;
														}
														$list1->size = $_len;
													}
												}
												break;
											case 24:
												if (($argCount != 1)) {
													$hasInterrupt = self::EX_InvalidArgument($ec, self::primitiveMethodWrongArgCountError("list remove method", 1, $argCount));
												} else {
													$value = $funcArgs->arr[0];
													if (($value->type != 3)) {
														$hasInterrupt = self::EX_InvalidArgument($ec, "Argument of list.remove must be an integer index.");
													} else {
														$int1 = $value->internalValue;
														$_len = $list1->size;
														if (($int1 < 0)) {
															$int1 += $_len;
														}
														if ((($int1 < 0) || ($int1 >= $_len))) {
															$hasInterrupt = self::EX_IndexOutOfRange($ec, "Index out of range.");
														} else {
															if ($returnValueUsed) {
																$output = $list1->list->arr[$int1];
															}
															$_len = ($list1->size - 1);
															$list1->size = $_len;
															array_splice($list1->list->arr, $int1, 1);
														}
													}
												}
												break;
											case 26:
												if (($argCount > 0)) {
													$hasInterrupt = self::EX_InvalidArgument($ec, self::primitiveMethodWrongArgCountError("list reverse method", 0, $argCount));
												} else {
													self::PST_reverseArray($list1->list);
												}
												break;
											case 28:
												if (($argCount > 0)) {
													$hasInterrupt = self::EX_InvalidArgument($ec, self::primitiveMethodWrongArgCountError("list shuffle method", 0, $argCount));
												} else {
													shuffle($list1->list->arr);
												}
												break;
											case 29:
												if (($argCount == 0)) {
													self::sortLists($list1, $list1, (self::PST_intBuffer16));
													if (((self::PST_intBuffer16)->arr[0] > 0)) {
														$hasInterrupt = self::EX_InvalidArgument($ec, "Invalid list to sort. All items must be numbers or all strings, but not mixed.");
													}
												} else if (($argCount == 1)) {
													$value2 = $funcArgs->arr[0];
													if (($value2->type == 9)) {
														$primitiveMethodToCoreLibraryFallback = true;
														$functionId = $metadata->primitiveMethodFunctionIdFallbackLookup->arr[2];
														$funcArgs->arr[1] = $value;
														$argCount = 2;
													} else {
														$hasInterrupt = self::EX_InvalidArgument($ec, "list.sort(get_key_function) requires a function pointer as its argument.");
													}
													$output = null;
												}
												break;
											default:
												$output = null;
												break;
										}
										break;
									case 7:
										// ...on a dictionary;
										$dictImpl = $value->internalValue;
										switch ($functionId) {
											case 4:
												if (($argCount > 0)) {
													$hasInterrupt = self::EX_InvalidArgument($ec, self::primitiveMethodWrongArgCountError("dictionary clear method", 0, $argCount));
												} else if (($dictImpl->size > 0)) {
													$dictImpl->intToIndex = new PastelPtrArray();
													$dictImpl->stringToIndex = new PastelPtrArray();
													$dictImpl->keys->arr = array();
													$dictImpl->values->arr = array();
													$dictImpl->size = 0;
												}
												break;
											case 5:
												if (($argCount > 0)) {
													$hasInterrupt = self::EX_InvalidArgument($ec, self::primitiveMethodWrongArgCountError("dictionary clone method", 0, $argCount));
												} else {
													$output = new Value(7, self::cloneDictionary($dictImpl, null));
												}
												break;
											case 7:
												if (($argCount != 1)) {
													$hasInterrupt = self::EX_InvalidArgument($ec, self::primitiveMethodWrongArgCountError("dictionary contains method", 1, $argCount));
												} else {
													$value = $funcArgs->arr[0];
													$output = $VALUE_FALSE;
													if (($value->type == 5)) {
														if (isset($dictImpl->stringToIndex->arr[$value->internalValue])) {
															$output = $VALUE_TRUE;
														}
													} else {
														if (($value->type == 3)) {
															$i = $value->internalValue;
														} else {
															$i = ($value->internalValue)->objectId;
														}
														if (isset($dictImpl->intToIndex->arr['i'.$i])) {
															$output = $VALUE_TRUE;
														}
													}
												}
												break;
											case 11:
												if ((($argCount != 1) && ($argCount != 2))) {
													$hasInterrupt = self::EX_InvalidArgument($ec, "Dictionary get method requires 1 or 2 arguments.");
												} else {
													$value = $funcArgs->arr[0];
													switch ($value->type) {
														case 3:
															$int1 = $value->internalValue;
															$i = isset($dictImpl->intToIndex->arr['i'.$int1]) ? $dictImpl->intToIndex->arr['i'.$int1] : (-1);
															break;
														case 8:
															$int1 = ($value->internalValue)->objectId;
															$i = isset($dictImpl->intToIndex->arr['i'.$int1]) ? $dictImpl->intToIndex->arr['i'.$int1] : (-1);
															break;
														case 5:
															$string1 = $value->internalValue;
															$i = isset($dictImpl->stringToIndex->arr[$string1]) ? $dictImpl->stringToIndex->arr[$string1] : (-1);
															break;
													}
													if (($i == -1)) {
														if (($argCount == 2)) {
															$output = $funcArgs->arr[1];
														} else {
															$output = $VALUE_NULL;
														}
													} else {
														$output = $dictImpl->values->arr[$i];
													}
												}
												break;
											case 18:
												if (($argCount > 0)) {
													$hasInterrupt = self::EX_InvalidArgument($ec, self::primitiveMethodWrongArgCountError("dictionary keys method", 0, $argCount));
												} else {
													$valueList1 = $dictImpl->keys;
													$_len = count($valueList1->arr);
													if (($dictImpl->keyType == 8)) {
														$intArray1 = pastelWrapList(array_fill(0, 2, 0));
														$intArray1->arr[0] = 8;
														$intArray1->arr[0] = $dictImpl->keyClassId;
													} else {
														$intArray1 = pastelWrapList(array_fill(0, 1, 0));
														$intArray1->arr[0] = $dictImpl->keyType;
													}
													$list1 = self::makeEmptyList($intArray1, $_len);
													$i = 0;
													while (($i < $_len)) {
														array_push($list1->list->arr, $valueList1->arr[$i]);
														$i += 1;
													}
													$list1->size = $_len;
													$output = new Value(6, $list1);
												}
												break;
											case 22:
												if (($argCount != 1)) {
													$hasInterrupt = self::EX_InvalidArgument($ec, self::primitiveMethodWrongArgCountError("dictionary merge method", 1, $argCount));
												} else {
													$value2 = $funcArgs->arr[0];
													if (($value2->type != 7)) {
														$hasInterrupt = self::EX_InvalidArgument($ec, "dictionary merge method requires another dictionary as a parameeter.");
													} else {
														$dictImpl2 = $value2->internalValue;
														if (($dictImpl2->size > 0)) {
															if (($dictImpl->size == 0)) {
																$value->internalValue = self::cloneDictionary($dictImpl2, null);
															} else if (($dictImpl2->keyType != $dictImpl->keyType)) {
																$hasInterrupt = self::EX_InvalidKey($ec, "Dictionaries with different key types cannot be merged.");
															} else if ((($dictImpl2->keyType == 8) && ($dictImpl2->keyClassId != $dictImpl->keyClassId) && ($dictImpl->keyClassId != 0) && !self::isClassASubclassOf($vm, $dictImpl2->keyClassId, $dictImpl->keyClassId))) {
																$hasInterrupt = self::EX_InvalidKey($ec, "Dictionary key types are incompatible.");
															} else {
																if (($dictImpl->valueType == null)) {
																} else if (($dictImpl2->valueType == null)) {
																	$hasInterrupt = self::EX_InvalidKey($ec, "Dictionaries with different value types cannot be merged.");
																} else if (!self::canAssignGenericToGeneric($vm, $dictImpl2->valueType, 0, $dictImpl->valueType, 0, $intBuffer)) {
																	$hasInterrupt = self::EX_InvalidKey($ec, "The dictionary value types are incompatible.");
																}
																if (!$hasInterrupt) {
																	self::cloneDictionary($dictImpl2, $dictImpl);
																}
															}
														}
														$output = $VALUE_NULL;
													}
												}
												break;
											case 24:
												if (($argCount != 1)) {
													$hasInterrupt = self::EX_InvalidArgument($ec, self::primitiveMethodWrongArgCountError("dictionary remove method", 1, $argCount));
												} else {
													$value2 = $funcArgs->arr[0];
													$bool2 = false;
													$keyType = $dictImpl->keyType;
													if ((($dictImpl->size > 0) && ($keyType == $value2->type))) {
														if (($keyType == 5)) {
															$stringKey = $value2->internalValue;
															if (isset($dictImpl->stringToIndex->arr[$stringKey])) {
																$i = $dictImpl->stringToIndex->arr[$stringKey];
																$bool2 = true;
															}
														} else {
															if (($keyType == 3)) {
																$intKey = $value2->internalValue;
															} else {
																$intKey = ($value2->internalValue)->objectId;
															}
															if (isset($dictImpl->intToIndex->arr['i'.$intKey])) {
																$i = $dictImpl->intToIndex->arr['i'.$intKey];
																$bool2 = true;
															}
														}
														if ($bool2) {
															$_len = ($dictImpl->size - 1);
															$dictImpl->size = $_len;
															if (($i == $_len)) {
																if (($keyType == 5)) {
																	unset($dictImpl->stringToIndex->arr[$stringKey]);
																} else {
																	unset($dictImpl->intToIndex->arr['i'.$intKey]);
																}
																array_splice($dictImpl->keys->arr, $i, 1);
																array_splice($dictImpl->values->arr, $i, 1);
															} else {
																$value = $dictImpl->keys->arr[$_len];
																self::PST_assignIndexHack($dictImpl->keys, $i, $value);
																self::PST_assignIndexHack($dictImpl->values, $i, $dictImpl->values->arr[$_len]);
																array_pop($dictImpl->keys->arr);
																array_pop($dictImpl->values->arr);
																if (($keyType == 5)) {
																	unset($dictImpl->stringToIndex->arr[$stringKey]);
																	$stringKey = $value->internalValue;
																	$dictImpl->stringToIndex->arr[$stringKey] = $i;
																} else {
																	unset($dictImpl->intToIndex->arr['i'.$intKey]);
																	if (($keyType == 3)) {
																		$intKey = $value->internalValue;
																	} else {
																		$intKey = ($value->internalValue)->objectId;
																	}
																	$dictImpl->intToIndex->arr['i'.$intKey] = $i;
																}
															}
														}
													}
													if (!$bool2) {
														$hasInterrupt = self::EX_KeyNotFound($ec, "dictionary does not contain the given key.");
													}
												}
												break;
											case 34:
												if (($argCount > 0)) {
													$hasInterrupt = self::EX_InvalidArgument($ec, self::primitiveMethodWrongArgCountError("dictionary values method", 0, $argCount));
												} else {
													$valueList1 = $dictImpl->values;
													$_len = count($valueList1->arr);
													$list1 = self::makeEmptyList($dictImpl->valueType, $_len);
													$i = 0;
													while (($i < $_len)) {
														self::addToList($list1, $valueList1->arr[$i]);
														$i += 1;
													}
													$output = new Value(6, $list1);
												}
												break;
											default:
												$output = null;
												break;
										}
										break;
									case 9:
										// ...on a function pointer;
										$functionPointer1 = $value->internalValue;
										switch ($functionId) {
											case 1:
												if (($argCount > 0)) {
													$hasInterrupt = self::EX_InvalidArgument($ec, self::primitiveMethodWrongArgCountError("argCountMax method", 0, $argCount));
												} else {
													$functionId = $functionPointer1->functionId;
													$functionInfo = $metadata->functionTable->arr[$functionId];
													$output = self::buildInteger($globals, $functionInfo->maxArgs);
												}
												break;
											case 2:
												if (($argCount > 0)) {
													$hasInterrupt = self::EX_InvalidArgument($ec, self::primitiveMethodWrongArgCountError("argCountMin method", 0, $argCount));
												} else {
													$functionId = $functionPointer1->functionId;
													$functionInfo = $metadata->functionTable->arr[$functionId];
													$output = self::buildInteger($globals, $functionInfo->minArgs);
												}
												break;
											case 12:
												$functionInfo = $metadata->functionTable->arr[$functionPointer1->functionId];
												$output = self::buildString($globals, $functionInfo->name);
												break;
											case 15:
												if (($argCount == 1)) {
													$funcArgs->arr[1] = $funcArgs->arr[0];
												} else if (($argCount == 0)) {
													$funcArgs->arr[1] = $VALUE_NULL;
												} else {
													$hasInterrupt = self::EX_InvalidArgument($ec, "invoke requires a list of arguments.");
												}
												$funcArgs->arr[0] = $value;
												$argCount = 2;
												$primitiveMethodToCoreLibraryFallback = true;
												$functionId = $metadata->primitiveMethodFunctionIdFallbackLookup->arr[3];
												$output = null;
												break;
											default:
												$output = null;
												break;
										}
										break;
									case 10:
										// ...on a class definition;
										$classValue = $value->internalValue;
										switch ($functionId) {
											case 12:
												$classInfo = $metadata->classTable->arr[$classValue->classId];
												$output = self::buildString($globals, $classInfo->fullyQualifiedName);
												break;
											case 16:
												if (($argCount != 1)) {
													$hasInterrupt = self::EX_InvalidArgument($ec, self::primitiveMethodWrongArgCountError("class isA method", 1, $argCount));
												} else {
													$int1 = $classValue->classId;
													$value = $funcArgs->arr[0];
													if (($value->type != 10)) {
														$hasInterrupt = self::EX_InvalidArgument($ec, "class isA method requires another class reference.");
													} else {
														$classValue = $value->internalValue;
														$int2 = $classValue->classId;
														$output = $VALUE_FALSE;
														if (self::isClassASubclassOf($vm, $int1, $int2)) {
															$output = $VALUE_TRUE;
														}
													}
												}
												break;
											default:
												$output = null;
												break;
										}
										break;
								}
								if (!$hasInterrupt) {
									if (($output == null)) {
										if ($primitiveMethodToCoreLibraryFallback) {
											$type = 1;
											$bool1 = true;
										} else {
											$hasInterrupt = self::EX_InvalidInvocation($ec, "primitive method not found.");
										}
									} else {
										if ($returnValueUsed) {
											if (($valueStackSize == $valueStackCapacity)) {
												$valueStack = self::valueStackIncreaseCapacity($ec);
												$valueStackCapacity = count($valueStack->arr);
											}
											$valueStack->arr[$valueStackSize] = $output;
											$valueStackSize += 1;
										}
										$bool1 = false;
									}
								}
							}
							if (($bool1 && !$hasInterrupt)) {
								// push a new frame to the stack;
								$stack->pc = $pc;
								$bool1 = false;
								switch ($type) {
									case 1:
										// function;
										$functionInfo = $functionTable->arr[$functionId];
										$pc = $functionInfo->pc;
										$value = null;
										$classId = 0;
										break;
									case 10:
										// lambda;
										$pc = $functionId;
										$functionInfo = $metadata->lambdaTable->arr['i'.$functionId];
										$value = null;
										$classId = 0;
										break;
									case 2:
										// static method;
										$functionInfo = $functionTable->arr[$functionId];
										$pc = $functionInfo->pc;
										$value = null;
										$classId = 0;
										break;
									case 3:
										// non-static method;
										$functionInfo = $functionTable->arr[$functionId];
										$pc = $functionInfo->pc;
										$classId = 0;
										break;
									case 6:
										// constructor;
										$vm->instanceCounter += 1;
										$classInfo = $classTable->arr[$classId];
										$valueArray1 = pastelWrapList(array_fill(0, $classInfo->memberCount, null));
										$i = (count($valueArray1->arr) - 1);
										while (($i >= 0)) {
											switch ($classInfo->fieldInitializationCommand->arr[$i]) {
												case 0:
													$valueArray1->arr[$i] = $classInfo->fieldInitializationLiteral->arr[$i];
													break;
												case 1:
													break;
												case 2:
													break;
											}
											$i -= 1;
										}
										$objInstance1 = new ObjectInstance($classId, $vm->instanceCounter, $valueArray1, null, null);
										$value = new Value(8, $objInstance1);
										$functionId = $classInfo->constructorFunctionId;
										$functionInfo = $functionTable->arr[$functionId];
										$pc = $functionInfo->pc;
										$classId = 0;
										if ($returnValueUsed) {
											$returnValueUsed = false;
											if (($valueStackSize == $valueStackCapacity)) {
												$valueStack = self::valueStackIncreaseCapacity($ec);
												$valueStackCapacity = count($valueStack->arr);
											}
											$valueStack->arr[$valueStackSize] = $value;
											$valueStackSize += 1;
										}
										break;
									case 7:
										// base constructor;
										$value = $stack->objectContext;
										$classInfo = $classTable->arr[$classId];
										$functionId = $classInfo->constructorFunctionId;
										$functionInfo = $functionTable->arr[$functionId];
										$pc = $functionInfo->pc;
										$classId = 0;
										break;
								}
								if ((($argCount < $functionInfo->minArgs) || ($argCount > $functionInfo->maxArgs))) {
									$pc = $stack->pc;
									$hasInterrupt = self::EX_InvalidArgument($ec, "Incorrect number of args were passed to this function.");
								} else {
									$int1 = $functionInfo->localsSize;
									$int2 = $stack->localsStackOffsetEnd;
									if (($localsStackCapacity <= ($int2 + $int1))) {
										self::increaseLocalsStackCapacity($ec, $int1);
										$localsStack = $ec->localsStack;
										$localsStackSet = $ec->localsStackSet;
										$localsStackCapacity = count($localsStack->arr);
									}
									$localsStackSetToken = ($ec->localsStackSetToken + 1);
									$ec->localsStackSetToken = $localsStackSetToken;
									if (($localsStackSetToken > 2000000000)) {
										self::resetLocalsStackTokens($ec, $stack);
										$localsStackSetToken = 2;
									}
									$localsStackOffset = $int2;
									if (($type == 10)) {
										$value = $closure->arr['i-1']->value;
									} else {
										$closure = null;
									}
									// invoke the function;
									$stack = new StackFrame($pc, $localsStackSetToken, $localsStackOffset, ($localsStackOffset + $int1), $stack, $returnValueUsed, $value, $valueStackSize, 0, ($stack->depth + 1), 0, null, $closure, null);
									$i = 0;
									while (($i < $argCount)) {
										$int1 = ($localsStackOffset + $i);
										$localsStack->arr[$int1] = $funcArgs->arr[$i];
										$localsStackSet->arr[$int1] = $localsStackSetToken;
										$i += 1;
									}
									if (($argCount != $functionInfo->minArgs)) {
										$int1 = ($argCount - $functionInfo->minArgs);
										if (($int1 > 0)) {
											$pc += $functionInfo->pcOffsetsForOptionalArgs->arr[$int1];
											$stack->pc = $pc;
										}
									}
									if (($stack->depth > 1000)) {
										$hasInterrupt = self::EX_Fatal($ec, "Stack overflow.");
									}
								}
							}
						}
						break;
					case 13:
						// CAST;
						$value = $valueStack->arr[($valueStackSize - 1)];
						$value2 = self::canAssignTypeToGeneric($vm, $value, $row, 0);
						if (($value2 == null)) {
							if ((($value->type == 4) && ($row->arr[0] == 3))) {
								if (($row->arr[1] == 1)) {
									$float1 = $value->internalValue;
									if ((($float1 < 0) && (($float1 % 1) != 0))) {
										$i = (intval($float1) - 1);
									} else {
										$i = intval($float1);
									}
									if (($i < 0)) {
										if (($i > -257)) {
											$value2 = $globals->negativeIntegers->arr[-$i];
										} else {
											$value2 = new Value(3, $i);
										}
									} else if (($i < 2049)) {
										$value2 = $globals->positiveIntegers->arr[$i];
									} else {
										$value2 = new Value(3, $i);
									}
								}
							} else if ((($value->type == 3) && ($row->arr[0] == 4))) {
								$int1 = $value->internalValue;
								if (($int1 == 0)) {
									$value2 = $VALUE_FLOAT_ZERO;
								} else {
									$value2 = new Value(4, (0.0 + $int1));
								}
							}
							if (($value2 != null)) {
								$valueStack->arr[($valueStackSize - 1)] = $value2;
							}
						}
						if (($value2 == null)) {
							$hasInterrupt = self::EX_InvalidArgument($ec, implode(array("Cannot convert a ", self::typeToStringFromValue($vm, $value), " to a ", self::typeToString($vm, $row, 0))));
						} else {
							$valueStack->arr[($valueStackSize - 1)] = $value2;
						}
						break;
					case 14:
						// CLASS_DEFINITION;
						self::initializeClass($pc, $vm, $row, $stringArgs->arr[$pc]);
						$classTable = $metadata->classTable;
						break;
					case 15:
						// CNI_INVOKE;
						$nativeFp = $metadata->cniFunctionsById->arr['i'.$row->arr[0]];
						if (($nativeFp == null)) {
							$hasInterrupt = self::EX_InvalidInvocation($ec, "CNI method could not be found.");
						} else {
							$_len = $row->arr[1];
							$valueStackSize -= $_len;
							$valueArray1 = pastelWrapList(array_fill(0, $_len, null));
							$i = 0;
							while (($i < $_len)) {
								$valueArray1->arr[$i] = $valueStack->arr[($valueStackSize + $i)];
								$i += 1;
							}
							self::prepareToSuspend($ec, $stack, $valueStackSize, $pc);
							$value = $nativeFp($vm, $valueArray1);
							if (($row->arr[2] == 1)) {
								if (($valueStackSize == $valueStackCapacity)) {
									$valueStack = self::valueStackIncreaseCapacity($ec);
									$valueStackCapacity = count($valueStack->arr);
								}
								$valueStack->arr[$valueStackSize] = $value;
								$valueStackSize += 1;
							}
							if ($ec->executionStateChange) {
								self::prepareToSuspend($ec, $stack, $valueStackSize, $pc);
								$ec->executionStateChange = false;
								if (($ec->executionStateChangeCommand == 1)) {
									return self::suspendInterpreter();
								}
							}
						}
						break;
					case 16:
						// CNI_REGISTER;
						$nativeFp = self::PST_createFunctionPointer($stringArgs->arr[$pc]);
						$metadata->cniFunctionsById->arr['i'.$row->arr[0]] = $nativeFp;
						break;
					case 17:
						// COMMAND_LINE_ARGS;
						if (($valueStackSize == $valueStackCapacity)) {
							$valueStack = self::valueStackIncreaseCapacity($ec);
							$valueStackCapacity = count($valueStack->arr);
						}
						$list1 = self::makeEmptyList($globals->stringType, 3);
						$i = 0;
						while (($i < count($vm->environment->commandLineArgs->arr))) {
							self::addToList($list1, self::buildString($globals, $vm->environment->commandLineArgs->arr[$i]));
							$i += 1;
						}
						$valueStack->arr[$valueStackSize] = new Value(6, $list1);
						$valueStackSize += 1;
						break;
					case 18:
						// CONTINUE;
						if (($row->arr[0] == 1)) {
							$pc += $row->arr[1];
						} else {
							$intArray1 = $esfData->arr[$pc];
							$pc = ($intArray1->arr[1] - 1);
							$valueStackSize = $stack->valueStackPopSize;
							$stack->postFinallyBehavior = 2;
						}
						break;
					case 19:
						// CORE_FUNCTION;
						switch ($row->arr[0]) {
							case 1:
								// parseInt;
								$arg1 = $valueStack->arr[--$valueStackSize];
								$output = $VALUE_NULL;
								if (($arg1->type == 5)) {
									$string1 = trim(($arg1->internalValue));
									if (self::PST_isValidInteger($string1)) {
										$output = self::buildInteger($globals, intval($string1));
									}
								} else {
									$hasInterrupt = self::EX_InvalidArgument($ec, "parseInt requires a string argument.");
								}
								break;
							case 2:
								// parseFloat;
								$arg1 = $valueStack->arr[--$valueStackSize];
								$output = $VALUE_NULL;
								if (($arg1->type == 5)) {
									$string1 = trim(($arg1->internalValue));
									self::PST_tryParseFloat($string1, $floatList1);
									if (($floatList1->arr[0] >= 0)) {
										$output = self::buildFloat($globals, $floatList1->arr[1]);
									}
								} else {
									$hasInterrupt = self::EX_InvalidArgument($ec, "parseFloat requires a string argument.");
								}
								break;
							case 3:
								// print;
								$arg1 = $valueStack->arr[--$valueStackSize];
								$output = $VALUE_NULL;
								self::printToStdOut($vm->environment->stdoutPrefix, self::valueToString($vm, $arg1));
								break;
							case 4:
								// typeof;
								$arg1 = $valueStack->arr[--$valueStackSize];
								$output = self::buildInteger($globals, ($arg1->type - 1));
								break;
							case 5:
								// typeis;
								$arg1 = $valueStack->arr[--$valueStackSize];
								$int1 = $arg1->type;
								$int2 = $row->arr[2];
								$output = $VALUE_FALSE;
								while (($int2 > 0)) {
									if (($row->arr[(2 + $int2)] == $int1)) {
										$output = $VALUE_TRUE;
										$int2 = 0;
									} else {
										$int2 -= 1;
									}
								}
								break;
							case 6:
								// execId;
								$output = self::buildInteger($globals, $ec->id);
								break;
							case 7:
								// assert;
								$valueStackSize -= 3;
								$arg3 = $valueStack->arr[($valueStackSize + 2)];
								$arg2 = $valueStack->arr[($valueStackSize + 1)];
								$arg1 = $valueStack->arr[$valueStackSize];
								if (($arg1->type != 2)) {
									$hasInterrupt = self::EX_InvalidArgument($ec, "Assertion expression must be a boolean.");
								} else if ($arg1->internalValue) {
									$output = $VALUE_NULL;
								} else {
									$string1 = self::valueToString($vm, $arg2);
									if ($arg3->internalValue) {
										$string1 = (("Assertion failed: ") . ($string1));
									}
									$hasInterrupt = self::EX_AssertionFailed($ec, $string1);
								}
								break;
							case 8:
								// chr;
								$arg1 = $valueStack->arr[--$valueStackSize];
								$output = null;
								if (($arg1->type == 3)) {
									$int1 = $arg1->internalValue;
									if ((($int1 >= 0) && ($int1 < 256))) {
										$output = self::buildCommonString($globals, chr($int1));
									}
								}
								if (($output == null)) {
									$hasInterrupt = self::EX_InvalidArgument($ec, "chr requires an integer between 0 and 255.");
								}
								break;
							case 9:
								// ord;
								$arg1 = $valueStack->arr[--$valueStackSize];
								$output = null;
								if (($arg1->type == 5)) {
									$string1 = $arg1->internalValue;
									if ((strlen($string1) == 1)) {
										$output = self::buildInteger($globals, chr($string1[0]));
									}
								}
								if (($output == null)) {
									$hasInterrupt = self::EX_InvalidArgument($ec, "ord requires a 1 character string.");
								}
								break;
							case 10:
								// currentTime;
								$output = self::buildFloat($globals, microtime(true));
								break;
							case 11:
								// sortList;
								$valueStackSize -= 2;
								$arg2 = $valueStack->arr[($valueStackSize + 1)];
								$arg1 = $valueStack->arr[$valueStackSize];
								$output = $VALUE_NULL;
								$list1 = $arg1->internalValue;
								$list2 = $arg2->internalValue;
								self::sortLists($list2, $list1, (self::PST_intBuffer16));
								if (((self::PST_intBuffer16)->arr[0] > 0)) {
									$hasInterrupt = self::EX_InvalidArgument($ec, "Invalid sort keys. Keys must be all numbers or all strings, but not mixed.");
								}
								break;
							case 12:
								// abs;
								$arg1 = $valueStack->arr[--$valueStackSize];
								$output = $arg1;
								if (($arg1->type == 3)) {
									if (($arg1->internalValue < 0)) {
										$output = self::buildInteger($globals, -$arg1->internalValue);
									}
								} else if (($arg1->type == 4)) {
									if (($arg1->internalValue < 0)) {
										$output = self::buildFloat($globals, -$arg1->internalValue);
									}
								} else {
									$hasInterrupt = self::EX_InvalidArgument($ec, "abs requires a number as input.");
								}
								break;
							case 13:
								// arcCos;
								$arg1 = $valueStack->arr[--$valueStackSize];
								if (($arg1->type == 4)) {
									$float1 = $arg1->internalValue;
								} else if (($arg1->type == 3)) {
									$float1 = (0.0 + $arg1->internalValue);
								} else {
									$hasInterrupt = self::EX_InvalidArgument($ec, "arccos requires a number as input.");
								}
								if (!$hasInterrupt) {
									if ((($float1 < -1) || ($float1 > 1))) {
										$hasInterrupt = self::EX_InvalidArgument($ec, "arccos requires a number in the range of -1 to 1.");
									} else {
										$output = self::buildFloat($globals, acos($float1));
									}
								}
								break;
							case 14:
								// arcSin;
								$arg1 = $valueStack->arr[--$valueStackSize];
								if (($arg1->type == 4)) {
									$float1 = $arg1->internalValue;
								} else if (($arg1->type == 3)) {
									$float1 = (0.0 + $arg1->internalValue);
								} else {
									$hasInterrupt = self::EX_InvalidArgument($ec, "arcsin requires a number as input.");
								}
								if (!$hasInterrupt) {
									if ((($float1 < -1) || ($float1 > 1))) {
										$hasInterrupt = self::EX_InvalidArgument($ec, "arcsin requires a number in the range of -1 to 1.");
									} else {
										$output = self::buildFloat($globals, asin($float1));
									}
								}
								break;
							case 15:
								// arcTan;
								$valueStackSize -= 2;
								$arg2 = $valueStack->arr[($valueStackSize + 1)];
								$arg1 = $valueStack->arr[$valueStackSize];
								$bool1 = false;
								if (($arg1->type == 4)) {
									$float1 = $arg1->internalValue;
								} else if (($arg1->type == 3)) {
									$float1 = (0.0 + $arg1->internalValue);
								} else {
									$bool1 = true;
								}
								if (($arg2->type == 4)) {
									$float2 = $arg2->internalValue;
								} else if (($arg2->type == 3)) {
									$float2 = (0.0 + $arg2->internalValue);
								} else {
									$bool1 = true;
								}
								if ($bool1) {
									$hasInterrupt = self::EX_InvalidArgument($ec, "arctan requires numeric arguments.");
								} else {
									$output = self::buildFloat($globals, atan2($float1, $float2));
								}
								break;
							case 16:
								// cos;
								$arg1 = $valueStack->arr[--$valueStackSize];
								if (($arg1->type == 4)) {
									$float1 = $arg1->internalValue;
									$output = self::buildFloat($globals, cos($float1));
								} else if (($arg1->type == 3)) {
									$int1 = $arg1->internalValue;
									$output = self::buildFloat($globals, cos($int1));
								} else {
									$hasInterrupt = self::EX_InvalidArgument($ec, "cos requires a number argument.");
								}
								break;
							case 17:
								// ensureRange;
								$valueStackSize -= 3;
								$arg3 = $valueStack->arr[($valueStackSize + 2)];
								$arg2 = $valueStack->arr[($valueStackSize + 1)];
								$arg1 = $valueStack->arr[$valueStackSize];
								$bool1 = false;
								if (($arg2->type == 4)) {
									$float2 = $arg2->internalValue;
								} else if (($arg2->type == 3)) {
									$float2 = (0.0 + $arg2->internalValue);
								} else {
									$bool1 = true;
								}
								if (($arg3->type == 4)) {
									$float3 = $arg3->internalValue;
								} else if (($arg3->type == 3)) {
									$float3 = (0.0 + $arg3->internalValue);
								} else {
									$bool1 = true;
								}
								if ((!$bool1 && ($float3 < $float2))) {
									$float1 = $float3;
									$float3 = $float2;
									$float2 = $float1;
									$value = $arg2;
									$arg2 = $arg3;
									$arg3 = $value;
								}
								if (($arg1->type == 4)) {
									$float1 = $arg1->internalValue;
								} else if (($arg1->type == 3)) {
									$float1 = (0.0 + $arg1->internalValue);
								} else {
									$bool1 = true;
								}
								if ($bool1) {
									$hasInterrupt = self::EX_InvalidArgument($ec, "ensureRange requires numeric arguments.");
								} else if (($float1 < $float2)) {
									$output = $arg2;
								} else if (($float1 > $float3)) {
									$output = $arg3;
								} else {
									$output = $arg1;
								}
								break;
							case 18:
								// floor;
								$arg1 = $valueStack->arr[--$valueStackSize];
								if (($arg1->type == 4)) {
									$float1 = $arg1->internalValue;
									if ((($float1 < 0) && (($float1 % 1) != 0))) {
										$int1 = (intval($float1) - 1);
									} else {
										$int1 = intval($float1);
									}
									if (($int1 < 2049)) {
										if (($int1 >= 0)) {
											$output = $INTEGER_POSITIVE_CACHE->arr[$int1];
										} else if (($int1 > -257)) {
											$output = $INTEGER_NEGATIVE_CACHE->arr[-$int1];
										} else {
											$output = new Value(3, $int1);
										}
									} else {
										$output = new Value(3, $int1);
									}
								} else if (($arg1->type == 3)) {
									$output = $arg1;
								} else {
									$hasInterrupt = self::EX_InvalidArgument($ec, "floor expects a numeric argument.");
								}
								break;
							case 19:
								// max;
								$valueStackSize -= 2;
								$arg2 = $valueStack->arr[($valueStackSize + 1)];
								$arg1 = $valueStack->arr[$valueStackSize];
								$bool1 = false;
								if (($arg1->type == 4)) {
									$float1 = $arg1->internalValue;
								} else if (($arg1->type == 3)) {
									$float1 = (0.0 + $arg1->internalValue);
								} else {
									$bool1 = true;
								}
								if (($arg2->type == 4)) {
									$float2 = $arg2->internalValue;
								} else if (($arg2->type == 3)) {
									$float2 = (0.0 + $arg2->internalValue);
								} else {
									$bool1 = true;
								}
								if ($bool1) {
									$hasInterrupt = self::EX_InvalidArgument($ec, "max requires numeric arguments.");
								} else if (($float1 >= $float2)) {
									$output = $arg1;
								} else {
									$output = $arg2;
								}
								break;
							case 20:
								// min;
								$valueStackSize -= 2;
								$arg2 = $valueStack->arr[($valueStackSize + 1)];
								$arg1 = $valueStack->arr[$valueStackSize];
								$bool1 = false;
								if (($arg1->type == 4)) {
									$float1 = $arg1->internalValue;
								} else if (($arg1->type == 3)) {
									$float1 = (0.0 + $arg1->internalValue);
								} else {
									$bool1 = true;
								}
								if (($arg2->type == 4)) {
									$float2 = $arg2->internalValue;
								} else if (($arg2->type == 3)) {
									$float2 = (0.0 + $arg2->internalValue);
								} else {
									$bool1 = true;
								}
								if ($bool1) {
									$hasInterrupt = self::EX_InvalidArgument($ec, "min requires numeric arguments.");
								} else if (($float1 <= $float2)) {
									$output = $arg1;
								} else {
									$output = $arg2;
								}
								break;
							case 21:
								// nativeInt;
								$valueStackSize -= 2;
								$arg2 = $valueStack->arr[($valueStackSize + 1)];
								$arg1 = $valueStack->arr[$valueStackSize];
								$output = self::buildInteger($globals, ($arg1->internalValue)->nativeData->arr[$arg2->internalValue]);
								break;
							case 22:
								// nativeString;
								$valueStackSize -= 3;
								$arg3 = $valueStack->arr[($valueStackSize + 2)];
								$arg2 = $valueStack->arr[($valueStackSize + 1)];
								$arg1 = $valueStack->arr[$valueStackSize];
								$string1 = ($arg1->internalValue)->nativeData->arr[$arg2->internalValue];
								if ($arg3->internalValue) {
									$output = self::buildCommonString($globals, $string1);
								} else {
									$output = self::buildString($globals, $string1);
								}
								break;
							case 23:
								// sign;
								$arg1 = $valueStack->arr[--$valueStackSize];
								if (($arg1->type == 3)) {
									$float1 = (0.0 + ($arg1->internalValue));
								} else if (($arg1->type == 4)) {
									$float1 = $arg1->internalValue;
								} else {
									$hasInterrupt = self::EX_InvalidArgument($ec, "sign requires a number as input.");
								}
								if (($float1 == 0)) {
									$output = $VALUE_INT_ZERO;
								} else if (($float1 > 0)) {
									$output = $VALUE_INT_ONE;
								} else {
									$output = $INTEGER_NEGATIVE_CACHE->arr[1];
								}
								break;
							case 24:
								// sin;
								$arg1 = $valueStack->arr[--$valueStackSize];
								if (($arg1->type == 4)) {
									$float1 = $arg1->internalValue;
								} else if (($arg1->type == 3)) {
									$float1 = (0.0 + $arg1->internalValue);
								} else {
									$hasInterrupt = self::EX_InvalidArgument($ec, "sin requires a number argument.");
								}
								$output = self::buildFloat($globals, sin($float1));
								break;
							case 25:
								// tan;
								$arg1 = $valueStack->arr[--$valueStackSize];
								if (($arg1->type == 4)) {
									$float1 = $arg1->internalValue;
								} else if (($arg1->type == 3)) {
									$float1 = (0.0 + $arg1->internalValue);
								} else {
									$hasInterrupt = self::EX_InvalidArgument($ec, "tan requires a number argument.");
								}
								if (!$hasInterrupt) {
									$float2 = cos($float1);
									if (($float2 < 0)) {
										$float2 = -$float2;
									}
									if (($float2 < 0.00000000015)) {
										$hasInterrupt = self::EX_DivisionByZero($ec, "Tangent is undefined.");
									} else {
										$output = self::buildFloat($globals, tan($float1));
									}
								}
								break;
							case 26:
								// log;
								$valueStackSize -= 2;
								$arg2 = $valueStack->arr[($valueStackSize + 1)];
								$arg1 = $valueStack->arr[$valueStackSize];
								if (($arg1->type == 4)) {
									$float1 = $arg1->internalValue;
								} else if (($arg1->type == 3)) {
									$float1 = (0.0 + $arg1->internalValue);
								} else {
									$hasInterrupt = self::EX_InvalidArgument($ec, "logarithms require a number argument.");
								}
								if (!$hasInterrupt) {
									if (($float1 <= 0)) {
										$hasInterrupt = self::EX_InvalidArgument($ec, "logarithms require positive inputs.");
									} else {
										$output = self::buildFloat($globals, self::fixFuzzyFloatPrecision((log($float1) * $arg2->internalValue)));
									}
								}
								break;
							case 27:
								// intQueueClear;
								$arg1 = $valueStack->arr[--$valueStackSize];
								$output = $VALUE_NULL;
								$objInstance1 = $arg1->internalValue;
								if (($objInstance1->nativeData != null)) {
									self::PST_assignIndexHack($objInstance1->nativeData, 1, 0);
								}
								break;
							case 28:
								// intQueueWrite16;
								$output = $VALUE_NULL;
								$int1 = $row->arr[2];
								$valueStackSize -= ($int1 + 1);
								$value = $valueStack->arr[$valueStackSize];
								$objArray1 = ($value->internalValue)->nativeData;
								$intArray1 = $objArray1->arr[0];
								$_len = $objArray1->arr[1];
								if (($_len >= count($intArray1->arr))) {
									$intArray2 = pastelWrapList(array_fill(0, (($_len * 2) + 16), 0));
									$j = 0;
									while (($j < $_len)) {
										$intArray2->arr[$j] = $intArray1->arr[$j];
										$j += 1;
									}
									$intArray1 = $intArray2;
									$objArray1->arr[0] = $intArray1;
								}
								$objArray1->arr[1] = ($_len + 16);
								$i = ($int1 - 1);
								while (($i >= 0)) {
									$value = $valueStack->arr[(($valueStackSize + 1) + $i)];
									if (($value->type == 3)) {
										$intArray1->arr[($_len + $i)] = $value->internalValue;
									} else if (($value->type == 4)) {
										$float1 = (0.5 + $value->internalValue);
										$intArray1->arr[($_len + $i)] = intval($float1);
									} else {
										$hasInterrupt = self::EX_InvalidArgument($ec, "Input must be integers.");
										$i = -1;
									}
									$i -= 1;
								}
								break;
							case 29:
								// execCounter;
								$output = self::buildInteger($globals, $ec->executionCounter);
								break;
							case 30:
								// sleep;
								$arg1 = $valueStack->arr[--$valueStackSize];
								$float1 = self::getFloat($arg1);
								if (($row->arr[1] == 1)) {
									if (($valueStackSize == $valueStackCapacity)) {
										$valueStack = self::valueStackIncreaseCapacity($ec);
										$valueStackCapacity = count($valueStack->arr);
									}
									$valueStack->arr[$valueStackSize] = $VALUE_NULL;
									$valueStackSize += 1;
								}
								self::prepareToSuspend($ec, $stack, $valueStackSize, $pc);
								$ec->activeInterrupt = new Interrupt(3, 0, "", $float1, null);
								$hasInterrupt = true;
								break;
							case 31:
								// projectId;
								$output = self::buildCommonString($globals, $metadata->projectId);
								break;
							case 32:
								// isJavaScript;
								$output = $VALUE_FALSE;
								break;
							case 33:
								// isAndroid;
								$output = $VALUE_FALSE;
								break;
							case 34:
								// allocNativeData;
								$valueStackSize -= 2;
								$arg2 = $valueStack->arr[($valueStackSize + 1)];
								$arg1 = $valueStack->arr[$valueStackSize];
								$objInstance1 = $arg1->internalValue;
								$int1 = $arg2->internalValue;
								$objArray1 = pastelWrapList(array_fill(0, $int1, null));
								$objInstance1->nativeData = $objArray1;
								break;
							case 35:
								// setNativeData;
								$valueStackSize -= 3;
								$arg3 = $valueStack->arr[($valueStackSize + 2)];
								$arg2 = $valueStack->arr[($valueStackSize + 1)];
								$arg1 = $valueStack->arr[$valueStackSize];
								self::PST_assignIndexHack(($arg1->internalValue)->nativeData, $arg2->internalValue, $arg3->internalValue);
								break;
							case 36:
								// getExceptionTrace;
								$arg1 = $valueStack->arr[--$valueStackSize];
								$intList1 = self::getNativeDataItem($arg1, 1);
								$list1 = self::makeEmptyList($globals->stringType, 20);
								$output = new Value(6, $list1);
								if (($intList1 != null)) {
									$stringList1 = self::tokenHelperConvertPcsToStackTraceStrings($vm, $intList1);
									$i = 0;
									while (($i < count($stringList1->arr))) {
										self::addToList($list1, self::buildString($globals, $stringList1->arr[$i]));
										$i += 1;
									}
									self::reverseList($list1);
								}
								break;
							case 37:
								// reflectAllClasses;
								$output = self::Reflect_allClasses($vm);
								break;
							case 38:
								// reflectGetMethods;
								$arg1 = $valueStack->arr[--$valueStackSize];
								$output = self::Reflect_getMethods($vm, $ec, $arg1);
								$hasInterrupt = ($ec->activeInterrupt != null);
								break;
							case 39:
								// reflectGetClass;
								$arg1 = $valueStack->arr[--$valueStackSize];
								if (($arg1->type != 8)) {
									$hasInterrupt = self::EX_InvalidArgument($ec, "Cannot get class from non-instance types.");
								} else {
									$objInstance1 = $arg1->internalValue;
									$output = new Value(10, new ClassValue(false, $objInstance1->classId));
								}
								break;
							case 40:
								// convertFloatArgsToInts;
								$int1 = $stack->localsStackOffsetEnd;
								$i = $localsStackOffset;
								while (($i < $int1)) {
									$value = $localsStack->arr[$i];
									if (($localsStackSet->arr[$i] != $localsStackSetToken)) {
										$i += $int1;
									} else if (($value->type == 4)) {
										$float1 = $value->internalValue;
										if ((($float1 < 0) && (($float1 % 1) != 0))) {
											$int2 = (intval($float1) - 1);
										} else {
											$int2 = intval($float1);
										}
										if ((($int2 >= 0) && ($int2 < 2049))) {
											$localsStack->arr[$i] = $INTEGER_POSITIVE_CACHE->arr[$int2];
										} else {
											$localsStack->arr[$i] = self::buildInteger($globals, $int2);
										}
									}
									$i += 1;
								}
								break;
							case 41:
								// addShutdownHandler;
								$arg1 = $valueStack->arr[--$valueStackSize];
								array_push($vm->shutdownHandlers->arr, $arg1);
								break;
						}
						if (($row->arr[1] == 1)) {
							if (($valueStackSize == $valueStackCapacity)) {
								$valueStack = self::valueStackIncreaseCapacity($ec);
								$valueStackCapacity = count($valueStack->arr);
							}
							$valueStack->arr[$valueStackSize] = $output;
							$valueStackSize += 1;
						}
						break;
					case 20:
						// DEBUG_SYMBOLS;
						self::applyDebugSymbolData($vm, $row, $stringArgs->arr[$pc], $metadata->mostRecentFunctionDef);
						break;
					case 21:
						// DEF_DICT;
						$intIntDict1 = new PastelPtrArray();
						$stringIntDict1 = new PastelPtrArray();
						$valueList2 = new PastelPtrArray();
						$valueList1 = new PastelPtrArray();
						$_len = $row->arr[0];
						$type = 3;
						$first = true;
						$i = $_len;
						while (($i > 0)) {
							$valueStackSize -= 2;
							$value = $valueStack->arr[($valueStackSize + 1)];
							$value2 = $valueStack->arr[$valueStackSize];
							if ($first) {
								$type = $value2->type;
								$first = false;
							} else if (($type != $value2->type)) {
								$hasInterrupt = self::EX_InvalidKey($ec, "Dictionary keys must be of the same type.");
							}
							if (!$hasInterrupt) {
								if (($type == 3)) {
									$intKey = $value2->internalValue;
								} else if (($type == 5)) {
									$stringKey = $value2->internalValue;
								} else if (($type == 8)) {
									$objInstance1 = $value2->internalValue;
									$intKey = $objInstance1->objectId;
								} else {
									$hasInterrupt = self::EX_InvalidKey($ec, "Only integers, strings, and objects can be used as dictionary keys.");
								}
							}
							if (!$hasInterrupt) {
								if (($type == 5)) {
									$stringIntDict1->arr[$stringKey] = count($valueList1->arr);
								} else {
									$intIntDict1->arr['i'.$intKey] = count($valueList1->arr);
								}
								array_push($valueList2->arr, $value2);
								array_push($valueList1->arr, $value);
								$i -= 1;
							}
						}
						if (!$hasInterrupt) {
							if (($type == 5)) {
								$i = count($stringIntDict1->arr);
							} else {
								$i = count($intIntDict1->arr);
							}
							if (($i != $_len)) {
								$hasInterrupt = self::EX_InvalidKey($ec, "Key collision");
							}
						}
						if (!$hasInterrupt) {
							$i = $row->arr[1];
							$classId = 0;
							if (($i > 0)) {
								$type = $row->arr[2];
								if (($type == 8)) {
									$classId = $row->arr[3];
								}
								$int1 = count($row->arr);
								$intArray1 = pastelWrapList(array_fill(0, ($int1 - $i), 0));
								while (($i < $int1)) {
									$intArray1->arr[($i - $row->arr[1])] = $row->arr[$i];
									$i += 1;
								}
							} else {
								$intArray1 = null;
							}
							if (($valueStackSize == $valueStackCapacity)) {
								$valueStack = self::valueStackIncreaseCapacity($ec);
								$valueStackCapacity = count($valueStack->arr);
							}
							$valueStack->arr[$valueStackSize] = new Value(7, new DictImpl($_len, $type, $classId, $intArray1, $intIntDict1, $stringIntDict1, $valueList2, $valueList1));
							$valueStackSize += 1;
						}
						break;
					case 22:
						// DEF_LIST;
						$int1 = $row->arr[0];
						$list1 = self::makeEmptyList(null, $int1);
						if (($row->arr[1] != 0)) {
							$list1->type = pastelWrapList(array_fill(0, (count($row->arr) - 1), 0));
							$i = 1;
							while (($i < count($row->arr))) {
								self::PST_assignIndexHack($list1->type, ($i - 1), $row->arr[$i]);
								$i += 1;
							}
						}
						$list1->size = $int1;
						while (($int1 > 0)) {
							$valueStackSize -= 1;
							array_push($list1->list->arr, $valueStack->arr[$valueStackSize]);
							$int1 -= 1;
						}
						self::PST_reverseArray($list1->list);
						$value = new Value(6, $list1);
						if (($valueStackSize == $valueStackCapacity)) {
							$valueStack = self::valueStackIncreaseCapacity($ec);
							$valueStackCapacity = count($valueStack->arr);
						}
						$valueStack->arr[$valueStackSize] = $value;
						$valueStackSize += 1;
						break;
					case 23:
						// DEF_ORIGINAL_CODE;
						self::defOriginalCodeImpl($vm, $row, $stringArgs->arr[$pc]);
						break;
					case 24:
						// DEREF_CLOSURE;
						$bool1 = true;
						$closure = $stack->closureVariables;
						$i = $row->arr[0];
						if ((($closure != null) && isset($closure->arr['i'.$i]))) {
							$value = $closure->arr['i'.$i]->value;
							if (($value != null)) {
								$bool1 = false;
								if (($valueStackSize == $valueStackCapacity)) {
									$valueStack = self::valueStackIncreaseCapacity($ec);
									$valueStackCapacity = count($valueStack->arr);
								}
								$valueStack->arr[$valueStackSize++] = $value;
							}
						}
						if ($bool1) {
							$hasInterrupt = self::EX_UnassignedVariable($ec, "Variable used before it was set.");
						}
						break;
					case 25:
						// DEREF_DOT;
						$value = $valueStack->arr[($valueStackSize - 1)];
						$nameId = $row->arr[0];
						$int2 = $row->arr[1];
						switch ($value->type) {
							case 8:
								$objInstance1 = $value->internalValue;
								$classId = $objInstance1->classId;
								$classInfo = $classTable->arr[$classId];
								if (($classId == $row->arr[4])) {
									$int1 = $row->arr[5];
								} else {
									$intIntDict1 = $classInfo->localeScopedNameIdToMemberId;
									$int1 = isset($intIntDict1->arr['i'.$int2]) ? $intIntDict1->arr['i'.$int2] : (-1);
									if (($int1 != -1)) {
										$int3 = $classInfo->fieldAccessModifiers->arr[$int1];
										if (($int3 > 1)) {
											if (($int3 == 2)) {
												if (($classId != $row->arr[2])) {
													$int1 = -2;
												}
											} else {
												if ((($int3 == 3) || ($int3 == 5))) {
													if (($classInfo->assemblyId != $row->arr[3])) {
														$int1 = -3;
													}
												}
												if ((($int3 == 4) || ($int3 == 5))) {
													$i = $row->arr[2];
													if (($classId == $i)) {
													} else {
														$classInfo = $classTable->arr[$classInfo->id];
														while ((($classInfo->baseClassId != -1) && ($int1 < count($classTable->arr[$classInfo->baseClassId]->fieldAccessModifiers->arr)))) {
															$classInfo = $classTable->arr[$classInfo->baseClassId];
														}
														$j = $classInfo->id;
														if (($j != $i)) {
															$bool1 = false;
															while ((($i != -1) && ($classTable->arr[$i]->baseClassId != -1))) {
																$i = $classTable->arr[$i]->baseClassId;
																if (($i == $j)) {
																	$bool1 = true;
																	$i = -1;
																}
															}
															if (!$bool1) {
																$int1 = -4;
															}
														}
													}
													$classInfo = $classTable->arr[$classId];
												}
											}
										}
										$row->arr[4] = $objInstance1->classId;
										$row->arr[5] = $int1;
									}
								}
								if (($int1 > -1)) {
									$functionId = $classInfo->functionIds->arr[$int1];
									if (($functionId == -1)) {
										$output = $objInstance1->members->arr[$int1];
									} else {
										$output = new Value(9, new FunctionPointer(2, $value, $objInstance1->classId, $functionId, null));
									}
								} else {
									$output = null;
								}
								break;
							case 5:
								if (($metadata->lengthId == $nameId)) {
									$output = self::buildInteger($globals, strlen(($value->internalValue)));
								} else {
									$output = null;
								}
								break;
							case 6:
								if (($metadata->lengthId == $nameId)) {
									$output = self::buildInteger($globals, ($value->internalValue)->size);
								} else {
									$output = null;
								}
								break;
							case 7:
								if (($metadata->lengthId == $nameId)) {
									$output = self::buildInteger($globals, ($value->internalValue)->size);
								} else {
									$output = null;
								}
								break;
							default:
								if (($value->type == 1)) {
									$hasInterrupt = self::EX_NullReference($ec, "Derferenced a field from null.");
									$output = $VALUE_NULL;
								} else {
									$output = null;
								}
								break;
						}
						if (($output == null)) {
							$output = self::generatePrimitiveMethodReference($globalNameIdToPrimitiveMethodName, $nameId, $value);
							if (($output == null)) {
								if (($value->type == 1)) {
									$hasInterrupt = self::EX_NullReference($ec, "Tried to dereference a field on null.");
								} else if ((($value->type == 8) && ($int1 < -1))) {
									$string1 = $identifiers->arr[$row->arr[0]];
									if (($int1 == -2)) {
										$string2 = "private";
									} else if (($int1 == -3)) {
										$string2 = "internal";
									} else {
										$string2 = "protected";
									}
									$hasInterrupt = self::EX_UnknownField($ec, implode(array("The field '", $string1, "' is marked as ", $string2, " and cannot be accessed from here.")));
								} else {
									if (($value->type == 8)) {
										$classId = ($value->internalValue)->classId;
										$classInfo = $classTable->arr[$classId];
										$string1 = (($classInfo->fullyQualifiedName) . (" instance"));
									} else {
										$string1 = self::getTypeFromId($value->type);
									}
									$hasInterrupt = self::EX_UnknownField($ec, (($string1) . (" does not have that field.")));
								}
							}
						}
						$valueStack->arr[($valueStackSize - 1)] = $output;
						break;
					case 26:
						// DEREF_INSTANCE_FIELD;
						$objInstance1 = $stack->objectContext->internalValue;
						$value = $objInstance1->members->arr[$row->arr[0]];
						if (($valueStackSize == $valueStackCapacity)) {
							$valueStack = self::valueStackIncreaseCapacity($ec);
							$valueStackCapacity = count($valueStack->arr);
						}
						$valueStack->arr[$valueStackSize++] = $value;
						break;
					case 27:
						// DEREF_STATIC_FIELD;
						$classInfo = $classTable->arr[$row->arr[0]];
						$staticConstructorNotInvoked = true;
						if (($classInfo->staticInitializationState < 2)) {
							$stack->pc = $pc;
							$stackFrame2 = self::maybeInvokeStaticConstructor($vm, $ec, $stack, $classInfo, $valueStackSize, (self::PST_intBuffer16));
							if (((self::PST_intBuffer16)->arr[0] == 1)) {
								return self::generateException($vm, $stack, $pc, $valueStackSize, $ec, 0, "Static initialization loop detected. The class this field is a member of is not done being initialized.");
							}
							if (($stackFrame2 != null)) {
								$staticConstructorNotInvoked = false;
								$stack = $stackFrame2;
								$pc = $stack->pc;
								$localsStackSetToken = $stack->localsStackSetToken;
								$localsStackOffset = $stack->localsStackOffset;
							}
						}
						if ($staticConstructorNotInvoked) {
							if (($valueStackSize == $valueStackCapacity)) {
								$valueStack = self::valueStackIncreaseCapacity($ec);
								$valueStackCapacity = count($valueStack->arr);
							}
							$valueStack->arr[$valueStackSize++] = $classInfo->staticFields->arr[$row->arr[1]];
						}
						break;
					case 28:
						// DUPLICATE_STACK_TOP;
						if (($row->arr[0] == 1)) {
							$value = $valueStack->arr[($valueStackSize - 1)];
							if (($valueStackSize == $valueStackCapacity)) {
								$valueStack = self::valueStackIncreaseCapacity($ec);
								$valueStackCapacity = count($valueStack->arr);
							}
							$valueStack->arr[$valueStackSize++] = $value;
						} else if (($row->arr[0] == 2)) {
							if ((($valueStackSize + 1) > $valueStackCapacity)) {
								self::valueStackIncreaseCapacity($ec);
								$valueStack = $ec->valueStack;
								$valueStackCapacity = count($valueStack->arr);
							}
							$valueStack->arr[$valueStackSize] = $valueStack->arr[($valueStackSize - 2)];
							$valueStack->arr[($valueStackSize + 1)] = $valueStack->arr[($valueStackSize - 1)];
							$valueStackSize += 2;
						} else {
							$hasInterrupt = self::EX_Fatal($ec, "?");
						}
						break;
					case 29:
						// EQUALS;
						$valueStackSize -= 2;
						$rightValue = $valueStack->arr[($valueStackSize + 1)];
						$leftValue = $valueStack->arr[$valueStackSize];
						if (($leftValue->type == $rightValue->type)) {
							switch ($leftValue->type) {
								case 1:
									$bool1 = true;
									break;
								case 2:
									$bool1 = ($leftValue->internalValue == $rightValue->internalValue);
									break;
								case 3:
									$bool1 = ($leftValue->internalValue == $rightValue->internalValue);
									break;
								case 5:
									$bool1 = ($leftValue->internalValue == $rightValue->internalValue);
									break;
								default:
									$bool1 = (self::doEqualityComparisonAndReturnCode($leftValue, $rightValue) == 1);
									break;
							}
						} else {
							$int1 = self::doEqualityComparisonAndReturnCode($leftValue, $rightValue);
							if (($int1 == 0)) {
								$bool1 = false;
							} else if (($int1 == 1)) {
								$bool1 = true;
							} else {
								$hasInterrupt = self::EX_UnsupportedOperation($ec, "== and != not defined here.");
							}
						}
						if (($valueStackSize == $valueStackCapacity)) {
							$valueStack = self::valueStackIncreaseCapacity($ec);
							$valueStackCapacity = count($valueStack->arr);
						}
						if (($bool1 != (($row->arr[0] == 1)))) {
							$valueStack->arr[$valueStackSize] = $VALUE_TRUE;
						} else {
							$valueStack->arr[$valueStackSize] = $VALUE_FALSE;
						}
						$valueStackSize += 1;
						break;
					case 30:
						// ESF_LOOKUP;
						$esfData = self::generateEsfData(count($args->arr), $row);
						$metadata->esfData = $esfData;
						break;
					case 31:
						// EXCEPTION_HANDLED_TOGGLE;
						$ec->activeExceptionHandled = ($row->arr[0] == 1);
						break;
					case 32:
						// FIELD_TYPE_INFO;
						self::initializeClassFieldTypeInfo($vm, $row);
						break;
					case 33:
						// FINALIZE_INITIALIZATION;
						self::finalizeInitializationImpl($vm, $stringArgs->arr[$pc], $row->arr[0]);
						$identifiers = $vm->metadata->identifiers;
						$literalTable = $vm->metadata->literalTable;
						$globalNameIdToPrimitiveMethodName = $vm->metadata->globalNameIdToPrimitiveMethodName;
						$funcArgs = $vm->funcArgs;
						break;
					case 34:
						// FINALLY_END;
						$value = $ec->activeException;
						if ((($value == null) || $ec->activeExceptionHandled)) {
							switch ($stack->postFinallyBehavior) {
								case 0:
									$ec->activeException = null;
									break;
								case 1:
									$ec->activeException = null;
									$int1 = $row->arr[0];
									if (($int1 == 1)) {
										$pc += $row->arr[1];
									} else if (($int1 == 2)) {
										$intArray1 = $esfData->arr[$pc];
										$pc = $intArray1->arr[1];
									} else {
										$hasInterrupt = self::EX_Fatal($ec, "break exists without a loop");
									}
									break;
								case 2:
									$ec->activeException = null;
									$int1 = $row->arr[2];
									if (($int1 == 1)) {
										$pc += $row->arr[3];
									} else if (($int1 == 2)) {
										$intArray1 = $esfData->arr[$pc];
										$pc = $intArray1->arr[1];
									} else {
										$hasInterrupt = self::EX_Fatal($ec, "continue exists without a loop");
									}
									break;
								case 3:
									if (($stack->markClassAsInitialized != 0)) {
										self::markClassAsInitialized($vm, $stack, $stack->markClassAsInitialized);
									}
									if ($stack->returnValueUsed) {
										$valueStackSize = $stack->valueStackPopSize;
										$value = $stack->returnValueTempStorage;
										$stack = $stack->previous;
										if (($valueStackSize == $valueStackCapacity)) {
											$valueStack = self::valueStackIncreaseCapacity($ec);
											$valueStackCapacity = count($valueStack->arr);
										}
										$valueStack->arr[$valueStackSize] = $value;
										$valueStackSize += 1;
									} else {
										$valueStackSize = $stack->valueStackPopSize;
										$stack = $stack->previous;
									}
									$pc = $stack->pc;
									$localsStackOffset = $stack->localsStackOffset;
									$localsStackSetToken = $stack->localsStackSetToken;
									break;
							}
						} else {
							$ec->activeExceptionHandled = false;
							$stack->pc = $pc;
							$intArray1 = $esfData->arr[$pc];
							$value = $ec->activeException;
							$objInstance1 = $value->internalValue;
							$objArray1 = $objInstance1->nativeData;
							$bool1 = true;
							if (($objArray1->arr[0] != null)) {
								$bool1 = $objArray1->arr[0];
							}
							$intList1 = $objArray1->arr[1];
							while ((($stack != null) && (($intArray1 == null) || $bool1))) {
								$stack = $stack->previous;
								if (($stack != null)) {
									$pc = $stack->pc;
									array_push($intList1->arr, $pc);
									$intArray1 = $esfData->arr[$pc];
								}
							}
							if (($stack == null)) {
								return self::uncaughtExceptionResult($vm, $value);
							}
							$int1 = $intArray1->arr[0];
							if (($int1 < $pc)) {
								$int1 = $intArray1->arr[1];
							}
							$pc = ($int1 - 1);
							$stack->pc = $pc;
							$localsStackOffset = $stack->localsStackOffset;
							$localsStackSetToken = $stack->localsStackSetToken;
							$ec->stackTop = $stack;
							$stack->postFinallyBehavior = 0;
							$ec->currentValueStackSize = $valueStackSize;
						}
						break;
					case 35:
						// FUNCTION_DEFINITION;
						self::initializeFunction($vm, $row, $pc, $stringArgs->arr[$pc]);
						$pc += $row->arr[7];
						$functionTable = $metadata->functionTable;
						break;
					case 36:
						// INDEX;
						$value = $valueStack->arr[--$valueStackSize];
						$root = $valueStack->arr[($valueStackSize - 1)];
						if (($root->type == 6)) {
							if (($value->type != 3)) {
								$hasInterrupt = self::EX_InvalidArgument($ec, "List index must be an integer.");
							} else {
								$i = $value->internalValue;
								$list1 = $root->internalValue;
								if (($i < 0)) {
									$i += $list1->size;
								}
								if ((($i < 0) || ($i >= $list1->size))) {
									$hasInterrupt = self::EX_IndexOutOfRange($ec, "List index is out of bounds");
								} else {
									$valueStack->arr[($valueStackSize - 1)] = $list1->list->arr[$i];
								}
							}
						} else if (($root->type == 7)) {
							$dictImpl = $root->internalValue;
							$keyType = $value->type;
							if (($keyType != $dictImpl->keyType)) {
								if (($dictImpl->size == 0)) {
									$hasInterrupt = self::EX_KeyNotFound($ec, "Key not found. Dictionary is empty.");
								} else {
									$hasInterrupt = self::EX_InvalidKey($ec, implode(array("Incorrect key type. This dictionary contains ", self::getTypeFromId($dictImpl->keyType), " keys. Provided key is a ", self::getTypeFromId($keyType), ".")));
								}
							} else {
								if (($keyType == 3)) {
									$intKey = $value->internalValue;
								} else if (($keyType == 5)) {
									$stringKey = $value->internalValue;
								} else if (($keyType == 8)) {
									$intKey = ($value->internalValue)->objectId;
								} else if (($dictImpl->size == 0)) {
									$hasInterrupt = self::EX_KeyNotFound($ec, "Key not found. Dictionary is empty.");
								} else {
									$hasInterrupt = self::EX_KeyNotFound($ec, "Key not found.");
								}
								if (!$hasInterrupt) {
									if (($keyType == 5)) {
										$stringIntDict1 = $dictImpl->stringToIndex;
										$int1 = isset($stringIntDict1->arr[$stringKey]) ? $stringIntDict1->arr[$stringKey] : (-1);
										if (($int1 == -1)) {
											$hasInterrupt = self::EX_KeyNotFound($ec, implode(array("Key not found: '", $stringKey, "'")));
										} else {
											$valueStack->arr[($valueStackSize - 1)] = $dictImpl->values->arr[$int1];
										}
									} else {
										$intIntDict1 = $dictImpl->intToIndex;
										$int1 = isset($intIntDict1->arr['i'.$intKey]) ? $intIntDict1->arr['i'.$intKey] : (-1);
										if (($int1 == -1)) {
											$hasInterrupt = self::EX_KeyNotFound($ec, "Key not found.");
										} else {
											$valueStack->arr[($valueStackSize - 1)] = $dictImpl->values->arr[$intIntDict1->arr['i'.$intKey]];
										}
									}
								}
							}
						} else if (($root->type == 5)) {
							$string1 = $root->internalValue;
							if (($value->type != 3)) {
								$hasInterrupt = self::EX_InvalidArgument($ec, "String indices must be integers.");
							} else {
								$int1 = $value->internalValue;
								if (($int1 < 0)) {
									$int1 += strlen($string1);
								}
								if ((($int1 < 0) || ($int1 >= strlen($string1)))) {
									$hasInterrupt = self::EX_IndexOutOfRange($ec, "String index out of range.");
								} else {
									$valueStack->arr[($valueStackSize - 1)] = self::buildCommonString($globals, $string1[$int1]);
								}
							}
						} else {
							$hasInterrupt = self::EX_InvalidArgument($ec, (("Cannot index into this type: ") . (self::getTypeFromId($root->type))));
						}
						break;
					case 37:
						// IS_COMPARISON;
						$value = $valueStack->arr[($valueStackSize - 1)];
						$output = $VALUE_FALSE;
						if (($value->type == 8)) {
							$objInstance1 = $value->internalValue;
							if (self::isClassASubclassOf($vm, $objInstance1->classId, $row->arr[0])) {
								$output = $VALUE_TRUE;
							}
						}
						$valueStack->arr[($valueStackSize - 1)] = $output;
						break;
					case 38:
						// ITERATION_STEP;
						$int1 = ($localsStackOffset + $row->arr[2]);
						$value3 = $localsStack->arr[$int1];
						$i = $value3->internalValue;
						$value = $localsStack->arr[($localsStackOffset + $row->arr[3])];
						if (($value->type == 6)) {
							$list1 = $value->internalValue;
							$_len = $list1->size;
							$bool1 = true;
						} else {
							$string2 = $value->internalValue;
							$_len = strlen($string2);
							$bool1 = false;
						}
						if (($i < $_len)) {
							if ($bool1) {
								$value = $list1->list->arr[$i];
							} else {
								$value = self::buildCommonString($globals, $string2[$i]);
							}
							$int3 = ($localsStackOffset + $row->arr[1]);
							$localsStackSet->arr[$int3] = $localsStackSetToken;
							$localsStack->arr[$int3] = $value;
						} else {
							$pc += $row->arr[0];
						}
						$i += 1;
						if (($i < 2049)) {
							$localsStack->arr[$int1] = $INTEGER_POSITIVE_CACHE->arr[$i];
						} else {
							$localsStack->arr[$int1] = new Value(3, $i);
						}
						break;
					case 39:
						// JUMP;
						$pc += $row->arr[0];
						break;
					case 40:
						// JUMP_IF_EXCEPTION_OF_TYPE;
						$value = $ec->activeException;
						$objInstance1 = $value->internalValue;
						$int1 = $objInstance1->classId;
						$i = (count($row->arr) - 1);
						while (($i >= 2)) {
							if (self::isClassASubclassOf($vm, $int1, $row->arr[$i])) {
								$i = 0;
								$pc += $row->arr[0];
								$int2 = $row->arr[1];
								if (($int2 > -1)) {
									$int1 = ($localsStackOffset + $int2);
									$localsStack->arr[$int1] = $value;
									$localsStackSet->arr[$int1] = $localsStackSetToken;
								}
							}
							$i -= 1;
						}
						break;
					case 41:
						// JUMP_IF_FALSE;
						$value = $valueStack->arr[--$valueStackSize];
						if (($value->type != 2)) {
							$hasInterrupt = self::EX_InvalidArgument($ec, "Boolean expected.");
						} else if (!$value->internalValue) {
							$pc += $row->arr[0];
						}
						break;
					case 42:
						// JUMP_IF_FALSE_NON_POP;
						$value = $valueStack->arr[($valueStackSize - 1)];
						if (($value->type != 2)) {
							$hasInterrupt = self::EX_InvalidArgument($ec, "Boolean expected.");
						} else if ($value->internalValue) {
							$valueStackSize -= 1;
						} else {
							$pc += $row->arr[0];
						}
						break;
					case 43:
						// JUMP_IF_TRUE;
						$value = $valueStack->arr[--$valueStackSize];
						if (($value->type != 2)) {
							$hasInterrupt = self::EX_InvalidArgument($ec, "Boolean expected.");
						} else if ($value->internalValue) {
							$pc += $row->arr[0];
						}
						break;
					case 44:
						// JUMP_IF_TRUE_NO_POP;
						$value = $valueStack->arr[($valueStackSize - 1)];
						if (($value->type != 2)) {
							$hasInterrupt = self::EX_InvalidArgument($ec, "Boolean expected.");
						} else if ($value->internalValue) {
							$pc += $row->arr[0];
						} else {
							$valueStackSize -= 1;
						}
						break;
					case 45:
						// LAMBDA;
						if (!isset($metadata->lambdaTable->arr['i'.$pc])) {
							$int1 = (4 + $row->arr[4] + 1);
							$_len = $row->arr[$int1];
							$intArray1 = pastelWrapList(array_fill(0, $_len, 0));
							$i = 0;
							while (($i < $_len)) {
								$intArray1->arr[$i] = $row->arr[($int1 + $i + 1)];
								$i += 1;
							}
							$_len = $row->arr[4];
							$intArray2 = pastelWrapList(array_fill(0, $_len, 0));
							$i = 0;
							while (($i < $_len)) {
								$intArray2->arr[$i] = $row->arr[(5 + $i)];
								$i += 1;
							}
							$metadata->lambdaTable->arr['i'.$pc] = new FunctionInfo($pc, 0, $pc, $row->arr[0], $row->arr[1], 5, 0, $row->arr[2], $intArray2, "lambda", $intArray1);
						}
						$closure = new PastelPtrArray();
						$parentClosure = $stack->closureVariables;
						if (($parentClosure == null)) {
							$parentClosure = new PastelPtrArray();
							$stack->closureVariables = $parentClosure;
							$parentClosure->arr['i-1'] = new ClosureValuePointer($stack->objectContext);
						}
						$closure->arr['i-1'] = $parentClosure->arr['i-1'];
						$functionInfo = $metadata->lambdaTable->arr['i'.$pc];
						$intArray1 = $functionInfo->closureIds;
						$_len = count($intArray1->arr);
						$i = 0;
						while (($i < $_len)) {
							$j = $intArray1->arr[$i];
							if (isset($parentClosure->arr['i'.$j])) {
								$closure->arr['i'.$j] = $parentClosure->arr['i'.$j];
							} else {
								$closure->arr['i'.$j] = new ClosureValuePointer(null);
								$parentClosure->arr['i'.$j] = $closure->arr['i'.$j];
							}
							$i += 1;
						}
						if (($valueStackSize == $valueStackCapacity)) {
							$valueStack = self::valueStackIncreaseCapacity($ec);
							$valueStackCapacity = count($valueStack->arr);
						}
						$valueStack->arr[$valueStackSize] = new Value(9, new FunctionPointer(5, null, 0, $pc, $closure));
						$valueStackSize += 1;
						$pc += $row->arr[3];
						break;
					case 46:
						// LIB_DECLARATION;
						self::prepareToSuspend($ec, $stack, $valueStackSize, $pc);
						$ec->activeInterrupt = new Interrupt(4, $row->arr[0], $stringArgs->arr[$pc], 0.0, null);
						$hasInterrupt = true;
						break;
					case 47:
						// LIST_SLICE;
						if (($row->arr[2] == 1)) {
							$valueStackSize -= 1;
							$arg3 = $valueStack->arr[$valueStackSize];
						} else {
							$arg3 = null;
						}
						if (($row->arr[1] == 1)) {
							$valueStackSize -= 1;
							$arg2 = $valueStack->arr[$valueStackSize];
						} else {
							$arg2 = null;
						}
						if (($row->arr[0] == 1)) {
							$valueStackSize -= 1;
							$arg1 = $valueStack->arr[$valueStackSize];
						} else {
							$arg1 = null;
						}
						$value = $valueStack->arr[($valueStackSize - 1)];
						$value = self::performListSlice($globals, $ec, $value, $arg1, $arg2, $arg3);
						$hasInterrupt = ($ec->activeInterrupt != null);
						if (!$hasInterrupt) {
							$valueStack->arr[($valueStackSize - 1)] = $value;
						}
						break;
					case 48:
						// LITERAL;
						if (($valueStackSize == $valueStackCapacity)) {
							$valueStack = self::valueStackIncreaseCapacity($ec);
							$valueStackCapacity = count($valueStack->arr);
						}
						$valueStack->arr[$valueStackSize++] = $literalTable->arr[$row->arr[0]];
						break;
					case 49:
						// LITERAL_STREAM;
						$int1 = count($row->arr);
						if ((($valueStackSize + $int1) > $valueStackCapacity)) {
							while ((($valueStackSize + $int1) > $valueStackCapacity)) {
								self::valueStackIncreaseCapacity($ec);
								$valueStack = $ec->valueStack;
								$valueStackCapacity = count($valueStack->arr);
							}
						}
						$i = $int1;
						while ((--$i >= 0)) {
							$valueStack->arr[$valueStackSize++] = $literalTable->arr[$row->arr[$i]];
						}
						break;
					case 50:
						// LOCAL;
						$int1 = ($localsStackOffset + $row->arr[0]);
						if (($localsStackSet->arr[$int1] == $localsStackSetToken)) {
							if (($valueStackSize == $valueStackCapacity)) {
								$valueStack = self::valueStackIncreaseCapacity($ec);
								$valueStackCapacity = count($valueStack->arr);
							}
							$valueStack->arr[$valueStackSize++] = $localsStack->arr[$int1];
						} else {
							$string1 = implode(array("Variable used before it was set: '", $vm->metadata->identifiers->arr[$row->arr[1]], "'"));
							$hasInterrupt = self::EX_UnassignedVariable($ec, $string1);
						}
						break;
					case 51:
						// LOC_TABLE;
						self::initLocTable($vm, $row);
						break;
					case 52:
						// NEGATIVE_SIGN;
						$value = $valueStack->arr[($valueStackSize - 1)];
						$type = $value->type;
						if (($type == 3)) {
							$valueStack->arr[($valueStackSize - 1)] = self::buildInteger($globals, -$value->internalValue);
						} else if (($type == 4)) {
							$valueStack->arr[($valueStackSize - 1)] = self::buildFloat($globals, -$value->internalValue);
						} else {
							$hasInterrupt = self::EX_InvalidArgument($ec, implode(array("Negative sign can only be applied to numbers. Found ", self::getTypeFromId($type), " instead.")));
						}
						break;
					case 53:
						// POP;
						$valueStackSize -= 1;
						break;
					case 54:
						// POP_IF_NULL_OR_JUMP;
						$value = $valueStack->arr[($valueStackSize - 1)];
						if (($value->type == 1)) {
							$valueStackSize -= 1;
						} else {
							$pc += $row->arr[0];
						}
						break;
					case 55:
						// PUSH_FUNC_REF;
						$value = null;
						switch ($row->arr[1]) {
							case 0:
								$value = new Value(9, new FunctionPointer(1, null, 0, $row->arr[0], null));
								break;
							case 1:
								$value = new Value(9, new FunctionPointer(2, $stack->objectContext, $row->arr[2], $row->arr[0], null));
								break;
							case 2:
								$classId = $row->arr[2];
								$classInfo = $classTable->arr[$classId];
								$staticConstructorNotInvoked = true;
								if (($classInfo->staticInitializationState < 2)) {
									$stack->pc = $pc;
									$stackFrame2 = self::maybeInvokeStaticConstructor($vm, $ec, $stack, $classInfo, $valueStackSize, (self::PST_intBuffer16));
									if (((self::PST_intBuffer16)->arr[0] == 1)) {
										return self::generateException($vm, $stack, $pc, $valueStackSize, $ec, 0, "Static initialization loop detected. The class this field is a member of is not done being initialized.");
									}
									if (($stackFrame2 != null)) {
										$staticConstructorNotInvoked = false;
										$stack = $stackFrame2;
										$pc = $stack->pc;
										$localsStackSetToken = $stack->localsStackSetToken;
										$localsStackOffset = $stack->localsStackOffset;
									}
								}
								if ($staticConstructorNotInvoked) {
									$value = new Value(9, new FunctionPointer(3, null, $classId, $row->arr[0], null));
								} else {
									$value = null;
								}
								break;
						}
						if (($value != null)) {
							if (($valueStackSize == $valueStackCapacity)) {
								$valueStack = self::valueStackIncreaseCapacity($ec);
								$valueStackCapacity = count($valueStack->arr);
							}
							$valueStack->arr[$valueStackSize] = $value;
							$valueStackSize += 1;
						}
						break;
					case 56:
						// RETURN;
						if (($esfData->arr[$pc] != null)) {
							$intArray1 = $esfData->arr[$pc];
							$pc = ($intArray1->arr[1] - 1);
							if (($row->arr[0] == 0)) {
								$stack->returnValueTempStorage = $VALUE_NULL;
							} else {
								$stack->returnValueTempStorage = $valueStack->arr[($valueStackSize - 1)];
							}
							$valueStackSize = $stack->valueStackPopSize;
							$stack->postFinallyBehavior = 3;
						} else {
							if (($stack->previous == null)) {
								return self::interpreterFinished($vm, $ec);
							}
							if (($stack->markClassAsInitialized != 0)) {
								self::markClassAsInitialized($vm, $stack, $stack->markClassAsInitialized);
							}
							if ($stack->returnValueUsed) {
								if (($row->arr[0] == 0)) {
									$valueStackSize = $stack->valueStackPopSize;
									$stack = $stack->previous;
									if (($valueStackSize == $valueStackCapacity)) {
										$valueStack = self::valueStackIncreaseCapacity($ec);
										$valueStackCapacity = count($valueStack->arr);
									}
									$valueStack->arr[$valueStackSize] = $VALUE_NULL;
								} else {
									$value = $valueStack->arr[($valueStackSize - 1)];
									$valueStackSize = $stack->valueStackPopSize;
									$stack = $stack->previous;
									$valueStack->arr[$valueStackSize] = $value;
								}
								$valueStackSize += 1;
							} else {
								$valueStackSize = $stack->valueStackPopSize;
								$stack = $stack->previous;
							}
							$pc = $stack->pc;
							$localsStackOffset = $stack->localsStackOffset;
							$localsStackSetToken = $stack->localsStackSetToken;
						}
						break;
					case 57:
						// STACK_INSERTION_FOR_INCREMENT;
						if (($valueStackSize == $valueStackCapacity)) {
							$valueStack = self::valueStackIncreaseCapacity($ec);
							$valueStackCapacity = count($valueStack->arr);
						}
						$valueStack->arr[$valueStackSize] = $valueStack->arr[($valueStackSize - 1)];
						$valueStack->arr[($valueStackSize - 1)] = $valueStack->arr[($valueStackSize - 2)];
						$valueStack->arr[($valueStackSize - 2)] = $valueStack->arr[($valueStackSize - 3)];
						$valueStack->arr[($valueStackSize - 3)] = $valueStack->arr[$valueStackSize];
						$valueStackSize += 1;
						break;
					case 58:
						// STACK_SWAP_POP;
						$valueStackSize -= 1;
						$valueStack->arr[($valueStackSize - 1)] = $valueStack->arr[$valueStackSize];
						break;
					case 59:
						// SWITCH_INT;
						$value = $valueStack->arr[--$valueStackSize];
						if (($value->type == 3)) {
							$intKey = $value->internalValue;
							$integerSwitch = $integerSwitchesByPc->arr[$pc];
							if (($integerSwitch == null)) {
								$integerSwitch = self::initializeIntSwitchStatement($vm, $pc, $row);
							}
							$i = isset($integerSwitch->arr['i'.$intKey]) ? $integerSwitch->arr['i'.$intKey] : (-1);
							if (($i == -1)) {
								$pc += $row->arr[0];
							} else {
								$pc += $i;
							}
						} else {
							$hasInterrupt = self::EX_InvalidArgument($ec, "Switch statement expects an integer.");
						}
						break;
					case 60:
						// SWITCH_STRING;
						$value = $valueStack->arr[--$valueStackSize];
						if (($value->type == 5)) {
							$stringKey = $value->internalValue;
							$stringSwitch = $stringSwitchesByPc->arr[$pc];
							if (($stringSwitch == null)) {
								$stringSwitch = self::initializeStringSwitchStatement($vm, $pc, $row);
							}
							$i = isset($stringSwitch->arr[$stringKey]) ? $stringSwitch->arr[$stringKey] : (-1);
							if (($i == -1)) {
								$pc += $row->arr[0];
							} else {
								$pc += $i;
							}
						} else {
							$hasInterrupt = self::EX_InvalidArgument($ec, "Switch statement expects a string.");
						}
						break;
					case 61:
						// THIS;
						if (($valueStackSize == $valueStackCapacity)) {
							$valueStack = self::valueStackIncreaseCapacity($ec);
							$valueStackCapacity = count($valueStack->arr);
						}
						$valueStack->arr[$valueStackSize] = $stack->objectContext;
						$valueStackSize += 1;
						break;
					case 62:
						// THROW;
						$valueStackSize -= 1;
						$value = $valueStack->arr[$valueStackSize];
						$bool2 = ($value->type == 8);
						if ($bool2) {
							$objInstance1 = $value->internalValue;
							if (!self::isClassASubclassOf($vm, $objInstance1->classId, $magicNumbers->coreExceptionClassId)) {
								$bool2 = false;
							}
						}
						if ($bool2) {
							$objArray1 = $objInstance1->nativeData;
							$intList1 = new PastelPtrArray();
							$objArray1->arr[1] = $intList1;
							if (!self::isPcFromCore($vm, $pc)) {
								array_push($intList1->arr, $pc);
							}
							$ec->activeException = $value;
							$ec->activeExceptionHandled = false;
							$stack->pc = $pc;
							$intArray1 = $esfData->arr[$pc];
							$value = $ec->activeException;
							$objInstance1 = $value->internalValue;
							$objArray1 = $objInstance1->nativeData;
							$bool1 = true;
							if (($objArray1->arr[0] != null)) {
								$bool1 = $objArray1->arr[0];
							}
							$intList1 = $objArray1->arr[1];
							while ((($stack != null) && (($intArray1 == null) || $bool1))) {
								$stack = $stack->previous;
								if (($stack != null)) {
									$pc = $stack->pc;
									array_push($intList1->arr, $pc);
									$intArray1 = $esfData->arr[$pc];
								}
							}
							if (($stack == null)) {
								return self::uncaughtExceptionResult($vm, $value);
							}
							$int1 = $intArray1->arr[0];
							if (($int1 < $pc)) {
								$int1 = $intArray1->arr[1];
							}
							$pc = ($int1 - 1);
							$stack->pc = $pc;
							$localsStackOffset = $stack->localsStackOffset;
							$localsStackSetToken = $stack->localsStackSetToken;
							$ec->stackTop = $stack;
							$stack->postFinallyBehavior = 0;
							$ec->currentValueStackSize = $valueStackSize;
						} else {
							$hasInterrupt = self::EX_InvalidArgument($ec, "Thrown value is not an exception.");
						}
						break;
					case 63:
						// TOKEN_DATA;
						self::tokenDataImpl($vm, $row);
						break;
					case 64:
						// USER_CODE_START;
						$metadata->userCodeStart = $row->arr[0];
						break;
					case 65:
						// VERIFY_TYPE_IS_ITERABLE;
						$value = $valueStack->arr[--$valueStackSize];
						$i = ($localsStackOffset + $row->arr[0]);
						$localsStack->arr[$i] = $value;
						$localsStackSet->arr[$i] = $localsStackSetToken;
						$int1 = $value->type;
						if ((($int1 != 6) && ($int1 != 5))) {
							$hasInterrupt = self::EX_InvalidArgument($ec, implode(array("Expected an iterable type, such as a list or string. Found ", self::getTypeFromId($int1), " instead.")));
						}
						$i = ($localsStackOffset + $row->arr[1]);
						$localsStack->arr[$i] = $VALUE_INT_ZERO;
						$localsStackSet->arr[$i] = $localsStackSetToken;
						break;
					default:
						// THIS SHOULD NEVER HAPPEN;
						return self::generateException($vm, $stack, $pc, $valueStackSize, $ec, 0, (("Bad op code: ") . (('' . ($ops->arr[$pc])))));
				}
				if ($hasInterrupt) {
					$interrupt = $ec->activeInterrupt;
					$ec->activeInterrupt = null;
					if (($interrupt->type == 1)) {
						return self::generateException($vm, $stack, $pc, $valueStackSize, $ec, $interrupt->exceptionType, $interrupt->exceptionMessage);
					}
					if (($interrupt->type == 3)) {
						return new InterpreterResult(5, "", $interrupt->sleepDurationSeconds, 0, false, "");
					}
					if (($interrupt->type == 4)) {
						return new InterpreterResult(6, "", 0.0, 0, false, $interrupt->exceptionMessage);
					}
				}
				++$pc;
			}
		}

		public static function invokeNamedCallback($vm, $id, $args) {
			$cb = $vm->namedCallbacks->callbacksById->arr[$id];
			return $cb($args);
		}

		public static function isClassASubclassOf($vm, $subClassId, $parentClassId) {
			if (($subClassId == $parentClassId)) {
				return true;
			}
			$classTable = $vm->metadata->classTable;
			$classIdWalker = $subClassId;
			while (($classIdWalker != -1)) {
				if (($classIdWalker == $parentClassId)) {
					return true;
				}
				$classInfo = $classTable->arr[$classIdWalker];
				$classIdWalker = $classInfo->baseClassId;
			}
			return false;
		}

		public static function isPcFromCore($vm, $pc) {
			if (($vm->symbolData == null)) {
				return false;
			}
			$tokens = $vm->symbolData->tokenData->arr[$pc];
			if (($tokens == null)) {
				return false;
			}
			$token = $tokens->arr[0];
			$filename = self::tokenHelperGetFileLine($vm, $token->fileId, 0);
			return "[Core]" === $filename;
		}

		public static function isStringEqual($a, $b) {
			if (($a == $b)) {
				return true;
			}
			return false;
		}

		public static function isVmResultRootExecContext($result) {
			return $result->isRootContext;
		}

		public static function makeEmptyList($type, $capacity) {
			return new ListImpl($type, 0, new PastelPtrArray());
		}

		public static function markClassAsInitialized($vm, $stack, $classId) {
			$classInfo = $vm->metadata->classTable->arr[$stack->markClassAsInitialized];
			$classInfo->staticInitializationState = 2;
			array_pop($vm->classStaticInitializationStack->arr);
			return 0;
		}

		public static function maybeInvokeStaticConstructor($vm, $ec, $stack, $classInfo, $valueStackSize, $intOutParam) {
			self::PST_assignIndexHack((self::PST_intBuffer16), 0, 0);
			$classId = $classInfo->id;
			if (($classInfo->staticInitializationState == 1)) {
				$classIdsBeingInitialized = $vm->classStaticInitializationStack;
				if (($classIdsBeingInitialized->arr[(count($classIdsBeingInitialized->arr) - 1)] != $classId)) {
					self::PST_assignIndexHack((self::PST_intBuffer16), 0, 1);
				}
				return null;
			}
			$classInfo->staticInitializationState = 1;
			array_push($vm->classStaticInitializationStack->arr, $classId);
			$functionInfo = $vm->metadata->functionTable->arr[$classInfo->staticConstructorFunctionId];
			$stack->pc -= 1;
			$newFrameLocalsSize = $functionInfo->localsSize;
			$currentFrameLocalsEnd = $stack->localsStackOffsetEnd;
			if ((count($ec->localsStack->arr) <= ($currentFrameLocalsEnd + $newFrameLocalsSize))) {
				self::increaseLocalsStackCapacity($ec, $newFrameLocalsSize);
			}
			if (($ec->localsStackSetToken > 2000000000)) {
				self::resetLocalsStackTokens($ec, $stack);
			}
			$ec->localsStackSetToken += 1;
			return new StackFrame($functionInfo->pc, $ec->localsStackSetToken, $currentFrameLocalsEnd, ($currentFrameLocalsEnd + $newFrameLocalsSize), $stack, false, null, $valueStackSize, $classId, ($stack->depth + 1), 0, null, null, null);
		}

		public static function multiplyString($globals, $strValue, $str, $n) {
			if (($n <= 2)) {
				if (($n == 1)) {
					return $strValue;
				}
				if (($n == 2)) {
					return self::buildString($globals, (($str) . ($str)));
				}
				return $globals->stringEmpty;
			}
			$builder = new PastelPtrArray();
			while (($n > 0)) {
				$n -= 1;
				array_push($builder->arr, $str);
			}
			$str = implode("", $builder->arr);
			return self::buildString($globals, $str);
		}

		public static function nextPowerOf2($value) {
			if (((($value - 1) & $value) == 0)) {
				return $value;
			}
			$output = 1;
			while (($output < $value)) {
				$output *= 2;
			}
			return $output;
		}

		public static function noop() {
			return 0;
		}

		public static function performListSlice($globals, $ec, $value, $arg1, $arg2, $arg3) {
			$begin = 0;
			$end = 0;
			$step = 0;
			$length = 0;
			$i = 0;
			$isForward = false;
			$isString = false;
			$originalString = "";
			$originalList = null;
			$outputList = null;
			$outputString = null;
			$status = 0;
			if (($arg3 != null)) {
				if (($arg3->type == 3)) {
					$step = $arg3->internalValue;
					if (($step == 0)) {
						$status = 2;
					}
				} else {
					$status = 3;
					$step = 1;
				}
			} else {
				$step = 1;
			}
			$isForward = ($step > 0);
			if (($arg2 != null)) {
				if (($arg2->type == 3)) {
					$end = $arg2->internalValue;
				} else {
					$status = 3;
				}
			}
			if (($arg1 != null)) {
				if (($arg1->type == 3)) {
					$begin = $arg1->internalValue;
				} else {
					$status = 3;
				}
			}
			if (($value->type == 5)) {
				$isString = true;
				$originalString = $value->internalValue;
				$length = strlen($originalString);
			} else if (($value->type == 6)) {
				$isString = false;
				$originalList = $value->internalValue;
				$length = $originalList->size;
			} else {
				self::EX_InvalidArgument($ec, implode(array("Cannot apply slicing to ", self::getTypeFromId($value->type), ". Must be string or list.")));
				return $globals->valueNull;
			}
			if (($status >= 2)) {
				$msg = null;
				if ($isString) {
					$msg = "String";
				} else {
					$msg = "List";
				}
				if (($status == 3)) {
					$msg .= " slice indexes must be integers. Found ";
					if ((($arg1 != null) && ($arg1->type != 3))) {
						self::EX_InvalidArgument($ec, implode(array($msg, self::getTypeFromId($arg1->type), " for begin index.")));
						return $globals->valueNull;
					}
					if ((($arg2 != null) && ($arg2->type != 3))) {
						self::EX_InvalidArgument($ec, implode(array($msg, self::getTypeFromId($arg2->type), " for end index.")));
						return $globals->valueNull;
					}
					if ((($arg3 != null) && ($arg3->type != 3))) {
						self::EX_InvalidArgument($ec, implode(array($msg, self::getTypeFromId($arg3->type), " for step amount.")));
						return $globals->valueNull;
					}
					self::EX_InvalidArgument($ec, "Invalid slice arguments.");
					return $globals->valueNull;
				} else {
					self::EX_InvalidArgument($ec, (($msg) . (" slice step cannot be 0.")));
					return $globals->valueNull;
				}
			}
			$status = self::canonicalizeListSliceArgs((self::PST_intBuffer16), $arg1, $arg2, $begin, $end, $step, $length, $isForward);
			if (($status == 1)) {
				$begin = (self::PST_intBuffer16)->arr[0];
				$end = (self::PST_intBuffer16)->arr[1];
				if ($isString) {
					$outputString = new PastelPtrArray();
					if ($isForward) {
						if (($step == 1)) {
							return self::buildString($globals, substr($originalString, $begin, ($end - $begin)));
						} else {
							while (($begin < $end)) {
								array_push($outputString->arr, $originalString[$begin]);
								$begin += $step;
							}
						}
					} else {
						while (($begin > $end)) {
							array_push($outputString->arr, $originalString[$begin]);
							$begin += $step;
						}
					}
					$value = self::buildString($globals, implode("", $outputString->arr));
				} else {
					$outputList = self::makeEmptyList($originalList->type, 10);
					if ($isForward) {
						while (($begin < $end)) {
							self::addToList($outputList, $originalList->list->arr[$begin]);
							$begin += $step;
						}
					} else {
						while (($begin > $end)) {
							self::addToList($outputList, $originalList->list->arr[$begin]);
							$begin += $step;
						}
					}
					$value = new Value(6, $outputList);
				}
			} else if (($status == 0)) {
				if ($isString) {
					$value = $globals->stringEmpty;
				} else {
					$value = new Value(6, self::makeEmptyList($originalList->type, 0));
				}
			} else if (($status == 2)) {
				if (!$isString) {
					$outputList = self::makeEmptyList($originalList->type, $length);
					$i = 0;
					while (($i < $length)) {
						self::addToList($outputList, $originalList->list->arr[$i]);
						$i += 1;
					}
					$value = new Value(6, $outputList);
				}
			} else {
				$msg = null;
				if ($isString) {
					$msg = "String";
				} else {
					$msg = "List";
				}
				if (($status == 3)) {
					$msg .= " slice begin index is out of range.";
				} else if ($isForward) {
					$msg .= " slice begin index must occur before the end index when step is positive.";
				} else {
					$msg .= " slice begin index must occur after the end index when the step is negative.";
				}
				self::EX_IndexOutOfRange($ec, $msg);
				return $globals->valueNull;
			}
			return $value;
		}

		public static function prepareToSuspend($ec, $stack, $valueStackSize, $currentPc) {
			$ec->stackTop = $stack;
			$ec->currentValueStackSize = $valueStackSize;
			$stack->pc = ($currentPc + 1);
			return 0;
		}

		public static function primitiveMethodsInitializeLookup($nameLookups) {
			$length = count($nameLookups->arr);
			$lookup = pastelWrapList(array_fill(0, $length, 0));
			$i = 0;
			while (($i < $length)) {
				$lookup->arr[$i] = -1;
				$i += 1;
			}
			if (isset($nameLookups->arr["add"])) {
				$lookup->arr[$nameLookups->arr["add"]] = 0;
			}
			if (isset($nameLookups->arr["argCountMax"])) {
				$lookup->arr[$nameLookups->arr["argCountMax"]] = 1;
			}
			if (isset($nameLookups->arr["argCountMin"])) {
				$lookup->arr[$nameLookups->arr["argCountMin"]] = 2;
			}
			if (isset($nameLookups->arr["choice"])) {
				$lookup->arr[$nameLookups->arr["choice"]] = 3;
			}
			if (isset($nameLookups->arr["clear"])) {
				$lookup->arr[$nameLookups->arr["clear"]] = 4;
			}
			if (isset($nameLookups->arr["clone"])) {
				$lookup->arr[$nameLookups->arr["clone"]] = 5;
			}
			if (isset($nameLookups->arr["concat"])) {
				$lookup->arr[$nameLookups->arr["concat"]] = 6;
			}
			if (isset($nameLookups->arr["contains"])) {
				$lookup->arr[$nameLookups->arr["contains"]] = 7;
			}
			if (isset($nameLookups->arr["createInstance"])) {
				$lookup->arr[$nameLookups->arr["createInstance"]] = 8;
			}
			if (isset($nameLookups->arr["endsWith"])) {
				$lookup->arr[$nameLookups->arr["endsWith"]] = 9;
			}
			if (isset($nameLookups->arr["filter"])) {
				$lookup->arr[$nameLookups->arr["filter"]] = 10;
			}
			if (isset($nameLookups->arr["get"])) {
				$lookup->arr[$nameLookups->arr["get"]] = 11;
			}
			if (isset($nameLookups->arr["getName"])) {
				$lookup->arr[$nameLookups->arr["getName"]] = 12;
			}
			if (isset($nameLookups->arr["indexOf"])) {
				$lookup->arr[$nameLookups->arr["indexOf"]] = 13;
			}
			if (isset($nameLookups->arr["insert"])) {
				$lookup->arr[$nameLookups->arr["insert"]] = 14;
			}
			if (isset($nameLookups->arr["invoke"])) {
				$lookup->arr[$nameLookups->arr["invoke"]] = 15;
			}
			if (isset($nameLookups->arr["isA"])) {
				$lookup->arr[$nameLookups->arr["isA"]] = 16;
			}
			if (isset($nameLookups->arr["join"])) {
				$lookup->arr[$nameLookups->arr["join"]] = 17;
			}
			if (isset($nameLookups->arr["keys"])) {
				$lookup->arr[$nameLookups->arr["keys"]] = 18;
			}
			if (isset($nameLookups->arr["lower"])) {
				$lookup->arr[$nameLookups->arr["lower"]] = 19;
			}
			if (isset($nameLookups->arr["ltrim"])) {
				$lookup->arr[$nameLookups->arr["ltrim"]] = 20;
			}
			if (isset($nameLookups->arr["map"])) {
				$lookup->arr[$nameLookups->arr["map"]] = 21;
			}
			if (isset($nameLookups->arr["merge"])) {
				$lookup->arr[$nameLookups->arr["merge"]] = 22;
			}
			if (isset($nameLookups->arr["pop"])) {
				$lookup->arr[$nameLookups->arr["pop"]] = 23;
			}
			if (isset($nameLookups->arr["remove"])) {
				$lookup->arr[$nameLookups->arr["remove"]] = 24;
			}
			if (isset($nameLookups->arr["replace"])) {
				$lookup->arr[$nameLookups->arr["replace"]] = 25;
			}
			if (isset($nameLookups->arr["reverse"])) {
				$lookup->arr[$nameLookups->arr["reverse"]] = 26;
			}
			if (isset($nameLookups->arr["rtrim"])) {
				$lookup->arr[$nameLookups->arr["rtrim"]] = 27;
			}
			if (isset($nameLookups->arr["shuffle"])) {
				$lookup->arr[$nameLookups->arr["shuffle"]] = 28;
			}
			if (isset($nameLookups->arr["sort"])) {
				$lookup->arr[$nameLookups->arr["sort"]] = 29;
			}
			if (isset($nameLookups->arr["split"])) {
				$lookup->arr[$nameLookups->arr["split"]] = 30;
			}
			if (isset($nameLookups->arr["startsWith"])) {
				$lookup->arr[$nameLookups->arr["startsWith"]] = 31;
			}
			if (isset($nameLookups->arr["trim"])) {
				$lookup->arr[$nameLookups->arr["trim"]] = 32;
			}
			if (isset($nameLookups->arr["upper"])) {
				$lookup->arr[$nameLookups->arr["upper"]] = 33;
			}
			if (isset($nameLookups->arr["values"])) {
				$lookup->arr[$nameLookups->arr["values"]] = 34;
			}
			return $lookup;
		}

		public static function primitiveMethodWrongArgCountError($name, $expected, $actual) {
			$output = "";
			if (($expected == 0)) {
				$output = (($name) . (" does not accept any arguments."));
			} else if (($expected == 1)) {
				$output = (($name) . (" accepts exactly 1 argument."));
			} else {
				$output = implode(array($name, " requires ", ('' . ($expected)), " arguments."));
			}
			return implode(array($output, " Found: ", ('' . ($actual))));
		}

		public static function printToStdOut($prefix, $line) {
			if (($prefix == null)) {
				echo $line;
			} else {
				$canonical = str_replace("\r", "\n", str_replace("\r\n", "\n", $line));
				$lines = pastelWrapList(explode("\n", $canonical));
				$i = 0;
				while (($i < count($lines->arr))) {
					echo implode(array($prefix, ": ", $lines->arr[$i]));
					$i += 1;
				}
			}
			return 0;
		}

		public static function qsortHelper($keyStringList, $keyNumList, $indices, $isString, $startIndex, $endIndex) {
			if ((($endIndex - $startIndex) <= 0)) {
				return 0;
			}
			if ((($endIndex - $startIndex) == 1)) {
				if (self::sortHelperIsRevOrder($keyStringList, $keyNumList, $isString, $startIndex, $endIndex)) {
					self::sortHelperSwap($keyStringList, $keyNumList, $indices, $isString, $startIndex, $endIndex);
				}
				return 0;
			}
			$mid = (($endIndex + $startIndex) >> 1);
			self::sortHelperSwap($keyStringList, $keyNumList, $indices, $isString, $mid, $startIndex);
			$upperPointer = ($endIndex + 1);
			$lowerPointer = ($startIndex + 1);
			while (($upperPointer > $lowerPointer)) {
				if (self::sortHelperIsRevOrder($keyStringList, $keyNumList, $isString, $startIndex, $lowerPointer)) {
					$lowerPointer += 1;
				} else {
					$upperPointer -= 1;
					self::sortHelperSwap($keyStringList, $keyNumList, $indices, $isString, $lowerPointer, $upperPointer);
				}
			}
			$midIndex = ($lowerPointer - 1);
			self::sortHelperSwap($keyStringList, $keyNumList, $indices, $isString, $midIndex, $startIndex);
			self::qsortHelper($keyStringList, $keyNumList, $indices, $isString, $startIndex, ($midIndex - 1));
			self::qsortHelper($keyStringList, $keyNumList, $indices, $isString, ($midIndex + 1), $endIndex);
			return 0;
		}

		public static function queryValue($vm, $execId, $stackFrameOffset, $steps) {
			if (($execId == -1)) {
				$execId = $vm->lastExecutionContextId;
			}
			$ec = $vm->executionContexts->arr['i'.$execId];
			$stackFrame = $ec->stackTop;
			while (($stackFrameOffset > 0)) {
				$stackFrameOffset -= 1;
				$stackFrame = $stackFrame->previous;
			}
			$current = null;
			$i = 0;
			$j = 0;
			$len = count($steps->arr);
			$i = 0;
			while (($i < count($steps->arr))) {
				if ((($current == null) && ($i > 0))) {
					return null;
				}
				$step = $steps->arr[$i];
				if (self::isStringEqual(".", $step)) {
					return null;
				} else if (self::isStringEqual("this", $step)) {
					$current = $stackFrame->objectContext;
				} else if (self::isStringEqual("class", $step)) {
					return null;
				} else if (self::isStringEqual("local", $step)) {
					$i += 1;
					$step = $steps->arr[$i];
					$localNamesByFuncPc = $vm->symbolData->localVarNamesById;
					$localNames = null;
					if ((($localNamesByFuncPc == null) || (count($localNamesByFuncPc->arr) == 0))) {
						return null;
					}
					$j = $stackFrame->pc;
					while (($j >= 0)) {
						if (isset($localNamesByFuncPc->arr['i'.$j])) {
							$localNames = $localNamesByFuncPc->arr['i'.$j];
							$j = -1;
						}
						$j -= 1;
					}
					if (($localNames == null)) {
						return null;
					}
					$localId = -1;
					if (($localNames != null)) {
						$j = 0;
						while (($j < count($localNames->arr))) {
							if (self::isStringEqual($localNames->arr[$j], $step)) {
								$localId = $j;
								$j = count($localNames->arr);
							}
							$j += 1;
						}
					}
					if (($localId == -1)) {
						return null;
					}
					$localOffset = ($localId + $stackFrame->localsStackOffset);
					if (($ec->localsStackSet->arr[$localOffset] != $stackFrame->localsStackSetToken)) {
						return null;
					}
					$current = $ec->localsStack->arr[$localOffset];
				} else if (self::isStringEqual("index", $step)) {
					return null;
				} else if (self::isStringEqual("key-int", $step)) {
					return null;
				} else if (self::isStringEqual("key-str", $step)) {
					return null;
				} else if (self::isStringEqual("key-obj", $step)) {
					return null;
				} else {
					return null;
				}
				$i += 1;
			}
			return $current;
		}

		public static function read_integer($pindex, $raw, $length, $alphaNums) {
			$num = 0;
			$c = $raw[$pindex->arr[0]];
			$pindex->arr[0] = ($pindex->arr[0] + 1);
			if (($c == "%")) {
				$value = self::read_till($pindex, $raw, $length, "%");
				$num = intval($value);
			} else if (($c == "@")) {
				$num = self::read_integer($pindex, $raw, $length, $alphaNums);
				$num *= 62;
				$num += self::read_integer($pindex, $raw, $length, $alphaNums);
			} else if (($c == "#")) {
				$num = self::read_integer($pindex, $raw, $length, $alphaNums);
				$num *= 62;
				$num += self::read_integer($pindex, $raw, $length, $alphaNums);
				$num *= 62;
				$num += self::read_integer($pindex, $raw, $length, $alphaNums);
			} else if (($c == "^")) {
				$num = (-1 * self::read_integer($pindex, $raw, $length, $alphaNums));
			} else {
				// TODO: string.IndexOfChar(c);
				$num = self::PST_stringIndexOf($alphaNums, $c, 0);
				if (($num == -1)) {
				}
			}
			return $num;
		}

		public static function read_string($pindex, $raw, $length, $alphaNums) {
			$b64 = self::read_till($pindex, $raw, $length, "%");
			return base64_decode($b64, true);
		}

		public static function read_till($index, $raw, $length, $end) {
			$output = new PastelPtrArray();
			$ctn = true;
			$c = " ";
			while ($ctn) {
				$c = $raw[$index->arr[0]];
				if (($c == $end)) {
					$ctn = false;
				} else {
					array_push($output->arr, $c);
				}
				$index->arr[0] = ($index->arr[0] + 1);
			}
			return implode($output->arr);
		}

		public static function reallocIntArray($original, $requiredCapacity) {
			$oldSize = count($original->arr);
			$size = $oldSize;
			while (($size < $requiredCapacity)) {
				$size *= 2;
			}
			$output = pastelWrapList(array_fill(0, $size, 0));
			$i = 0;
			while (($i < $oldSize)) {
				$output->arr[$i] = $original->arr[$i];
				$i += 1;
			}
			return $output;
		}

		public static function Reflect_allClasses($vm) {
			$generics = pastelWrapList(array_fill(0, 1, 0));
			$generics->arr[0] = 10;
			$output = self::makeEmptyList($generics, 20);
			$classTable = $vm->metadata->classTable;
			$i = 1;
			while (($i < count($classTable->arr))) {
				$classInfo = $classTable->arr[$i];
				if (($classInfo == null)) {
					$i = count($classTable->arr);
				} else {
					self::addToList($output, new Value(10, new ClassValue(false, $classInfo->id)));
				}
				$i += 1;
			}
			return new Value(6, $output);
		}

		public static function Reflect_getMethods($vm, $ec, $methodSource) {
			$output = self::makeEmptyList(null, 8);
			if (($methodSource->type == 8)) {
				$objInstance1 = $methodSource->internalValue;
				$classInfo = $vm->metadata->classTable->arr[$objInstance1->classId];
				$i = 0;
				while (($i < count($classInfo->functionIds->arr))) {
					$functionId = $classInfo->functionIds->arr[$i];
					if (($functionId != -1)) {
						self::addToList($output, new Value(9, new FunctionPointer(2, $methodSource, $objInstance1->classId, $functionId, null)));
					}
					$i += 1;
				}
			} else {
				$classValue = $methodSource->internalValue;
				$classInfo = $vm->metadata->classTable->arr[$classValue->classId];
				self::EX_UnsupportedOperation($ec, "static method reflection not implemented yet.");
			}
			return new Value(6, $output);
		}

		public static function registerNamedCallback($vm, $scope, $functionName, $callback) {
			$id = self::getNamedCallbackIdImpl($vm, $scope, $functionName, true);
			self::PST_assignIndexHack($vm->namedCallbacks->callbacksById, $id, $callback);
			return $id;
		}

		public static function resetLocalsStackTokens($ec, $stack) {
			$localsStack = $ec->localsStack;
			$localsStackSet = $ec->localsStackSet;
			$i = $stack->localsStackOffsetEnd;
			while (($i < count($localsStackSet->arr))) {
				$localsStackSet->arr[$i] = 0;
				$localsStack->arr[$i] = null;
				$i += 1;
			}
			$stackWalker = $stack;
			while (($stackWalker != null)) {
				$token = $stackWalker->localsStackSetToken;
				$stackWalker->localsStackSetToken = 1;
				$i = $stackWalker->localsStackOffset;
				while (($i < $stackWalker->localsStackOffsetEnd)) {
					if (($localsStackSet->arr[$i] == $token)) {
						$localsStackSet->arr[$i] = 1;
					} else {
						$localsStackSet->arr[$i] = 0;
						$localsStack->arr[$i] = null;
					}
					$i += 1;
				}
				$stackWalker = $stackWalker->previous;
			}
			$ec->localsStackSetToken = 1;
			return -1;
		}

		public static function resolvePrimitiveMethodName2($lookup, $type, $globalNameId) {
			$output = $lookup->arr[$globalNameId];
			if (($output != -1)) {
				switch (($type + (11 * $output))) {
					case 82:
						return $output;
					case 104:
						return $output;
					case 148:
						return $output;
					case 214:
						return $output;
					case 225:
						return $output;
					case 280:
						return $output;
					case 291:
						return $output;
					case 302:
						return $output;
					case 335:
						return $output;
					case 346:
						return $output;
					case 357:
						return $output;
					case 368:
						return $output;
					case 6:
						return $output;
					case 39:
						return $output;
					case 50:
						return $output;
					case 61:
						return $output;
					case 72:
						return $output;
					case 83:
						return $output;
					case 116:
						return $output;
					case 160:
						return $output;
					case 193:
						return $output;
					case 237:
						return $output;
					case 259:
						return $output;
					case 270:
						return $output;
					case 292:
						return $output;
					case 314:
						return $output;
					case 325:
						return $output;
					case 51:
						return $output;
					case 62:
						return $output;
					case 84:
						return $output;
					case 128:
						return $output;
					case 205:
						return $output;
					case 249:
						return $output;
					case 271:
						return $output;
					case 381:
						return $output;
					case 20:
						return $output;
					case 31:
						return $output;
					case 141:
						return $output;
					case 174:
						return $output;
					case 98:
						return $output;
					case 142:
						return $output;
					case 186:
						return $output;
					default:
						return -1;
				}
			}
			return -1;
		}

		public static function resource_manager_getResourceOfType($vm, $userPath, $type) {
			$db = $vm->resourceDatabase;
			$lookup = $db->fileInfo;
			if (isset($lookup->arr[$userPath])) {
				$output = self::makeEmptyList(null, 2);
				$file = $lookup->arr[$userPath];
				if ($file->type === $type) {
					self::addToList($output, $vm->globals->boolTrue);
					self::addToList($output, self::buildString($vm->globals, $file->internalPath));
				} else {
					self::addToList($output, $vm->globals->boolFalse);
				}
				return new Value(6, $output);
			}
			return $vm->globals->valueNull;
		}

		public static function resource_manager_populate_directory_lookup($dirs, $path) {
			$parts = pastelWrapList(explode("/", $path));
			$pathBuilder = "";
			$file = "";
			$i = 0;
			while (($i < count($parts->arr))) {
				$file = $parts->arr[$i];
				$files = null;
				if (!isset($dirs->arr[$pathBuilder])) {
					$files = new PastelPtrArray();
					$dirs->arr[$pathBuilder] = $files;
				} else {
					$files = $dirs->arr[$pathBuilder];
				}
				array_push($files->arr, $file);
				if (($i > 0)) {
					$pathBuilder = implode(array($pathBuilder, "/", $file));
				} else {
					$pathBuilder = $file;
				}
				$i += 1;
			}
			return 0;
		}

		public static function resourceManagerInitialize($globals, $manifest) {
			$filesPerDirectoryBuilder = new PastelPtrArray();
			$fileInfo = new PastelPtrArray();
			$dataList = new PastelPtrArray();
			$items = pastelWrapList(explode("\n", $manifest));
			$resourceInfo = null;
			$type = "";
			$userPath = "";
			$internalPath = "";
			$argument = "";
			$isText = false;
			$intType = 0;
			$i = 0;
			while (($i < count($items->arr))) {
				$itemData = pastelWrapList(explode(",", $items->arr[$i]));
				if ((count($itemData->arr) >= 3)) {
					$type = $itemData->arr[0];
					$isText = "TXT" === $type;
					if ($isText) {
						$intType = 1;
					} else if (("IMGSH" === $type || "IMG" === $type)) {
						$intType = 2;
					} else if ("SND" === $type) {
						$intType = 3;
					} else if ("TTF" === $type) {
						$intType = 4;
					} else {
						$intType = 5;
					}
					$userPath = self::stringDecode($itemData->arr[1]);
					$internalPath = $itemData->arr[2];
					$argument = "";
					if ((count($itemData->arr) > 3)) {
						$argument = self::stringDecode($itemData->arr[3]);
					}
					$resourceInfo = new ResourceInfo($userPath, $internalPath, $isText, $type, $argument);
					$fileInfo->arr[$userPath] = $resourceInfo;
					self::resource_manager_populate_directory_lookup($filesPerDirectoryBuilder, $userPath);
					array_push($dataList->arr, self::buildString($globals, $userPath));
					array_push($dataList->arr, self::buildInteger($globals, $intType));
					if (($internalPath != null)) {
						array_push($dataList->arr, self::buildString($globals, $internalPath));
					} else {
						array_push($dataList->arr, $globals->valueNull);
					}
				}
				$i += 1;
			}
			$dirs = self::PST_dictGetKeys($filesPerDirectoryBuilder, false);
			$filesPerDirectorySorted = new PastelPtrArray();
			$i = 0;
			while (($i < count($dirs->arr))) {
				$dir = $dirs->arr[$i];
				$unsortedDirs = $filesPerDirectoryBuilder->arr[$dir];
				$dirsSorted = pastelWrapList($unsortedDirs->arr);
				$dirsSorted = self::PST_sortedCopyOfStringArray($dirsSorted);
				$filesPerDirectorySorted->arr[$dir] = $dirsSorted;
				$i += 1;
			}
			return new ResourceDB($filesPerDirectorySorted, $fileInfo, $dataList);
		}

		public static function reverseList($list) {
			self::PST_reverseArray($list->list);
		}

		public static function runInterpreter($vm, $executionContextId) {
			$result = self::interpret($vm, $executionContextId);
			$result->executionContextId = $executionContextId;
			$status = $result->status;
			if (($status == 1)) {
				if (isset($vm->executionContexts->arr['i'.$executionContextId])) {
					unset($vm->executionContexts->arr['i'.$executionContextId]);
				}
				self::runShutdownHandlers($vm);
			} else if (($status == 3)) {
				self::printToStdOut($vm->environment->stacktracePrefix, $result->errorMessage);
				self::runShutdownHandlers($vm);
			}
			if (($executionContextId == 0)) {
				$result->isRootContext = true;
			}
			return $result;
		}

		public static function runInterpreterWithFunctionPointer($vm, $fpValue, $args) {
			$newId = ($vm->lastExecutionContextId + 1);
			$vm->lastExecutionContextId = $newId;
			$argList = new PastelPtrArray();
			$i = 0;
			while (($i < count($args->arr))) {
				array_push($argList->arr, $args->arr[$i]);
				$i += 1;
			}
			$locals = pastelWrapList(array_fill(0, 0, null));
			$localsSet = pastelWrapList(array_fill(0, 0, 0));
			$valueStack = pastelWrapList(array_fill(0, 100, null));
			$valueStack->arr[0] = $fpValue;
			$valueStack->arr[1] = self::buildList($argList);
			$stack = new StackFrame((count($vm->byteCode->ops->arr) - 2), 1, 0, 0, null, false, null, 0, 0, 1, 0, null, null, null);
			$executionContext = new ExecutionContext($newId, $stack, 2, 100, $valueStack, $locals, $localsSet, 1, 0, false, null, false, 0, null);
			$vm->executionContexts->arr['i'.$newId] = $executionContext;
			return self::runInterpreter($vm, $newId);
		}

		public static function runShutdownHandlers($vm) {
			while ((count($vm->shutdownHandlers->arr) > 0)) {
				$handler = $vm->shutdownHandlers->arr[0];
				array_splice($vm->shutdownHandlers->arr, 0, 1);
				self::runInterpreterWithFunctionPointer($vm, $handler, pastelWrapList(array_fill(0, 0, null)));
			}
			return 0;
		}

		public static function setItemInList($list, $i, $v) {
			self::PST_assignIndexHack($list->list, $i, $v);
		}

		public static function sortHelperIsRevOrder($keyStringList, $keyNumList, $isString, $indexLeft, $indexRight) {
			if ($isString) {
				return (strcmp($keyStringList->arr[$indexLeft], $keyStringList->arr[$indexRight]) > 0);
			}
			return ($keyNumList->arr[$indexLeft] > $keyNumList->arr[$indexRight]);
		}

		public static function sortHelperSwap($keyStringList, $keyNumList, $indices, $isString, $index1, $index2) {
			if (($index1 == $index2)) {
				return 0;
			}
			$t = $indices->arr[$index1];
			$indices->arr[$index1] = $indices->arr[$index2];
			$indices->arr[$index2] = $t;
			if ($isString) {
				$s = $keyStringList->arr[$index1];
				$keyStringList->arr[$index1] = $keyStringList->arr[$index2];
				$keyStringList->arr[$index2] = $s;
			} else {
				$n = $keyNumList->arr[$index1];
				$keyNumList->arr[$index1] = $keyNumList->arr[$index2];
				$keyNumList->arr[$index2] = $n;
			}
			return 0;
		}

		public static function sortLists($keyList, $parallelList, $intOutParam) {
			self::PST_assignIndexHack((self::PST_intBuffer16), 0, 0);
			$length = $keyList->size;
			if (($length < 2)) {
				return 0;
			}
			$i = 0;
			$item = null;
			$item = $keyList->list->arr[0];
			$isString = ($item->type == 5);
			$stringKeys = null;
			$numKeys = null;
			if ($isString) {
				$stringKeys = pastelWrapList(array_fill(0, $length, null));
			} else {
				$numKeys = pastelWrapList(array_fill(0, $length, 0.0));
			}
			$indices = pastelWrapList(array_fill(0, $length, 0));
			$originalOrder = pastelWrapList(array_fill(0, $length, null));
			$i = 0;
			while (($i < $length)) {
				$indices->arr[$i] = $i;
				$originalOrder->arr[$i] = $parallelList->list->arr[$i];
				$item = $keyList->list->arr[$i];
				switch ($item->type) {
					case 3:
						if ($isString) {
							self::PST_assignIndexHack((self::PST_intBuffer16), 0, 1);
							return 0;
						}
						$numKeys->arr[$i] = $item->internalValue;
						break;
					case 4:
						if ($isString) {
							self::PST_assignIndexHack((self::PST_intBuffer16), 0, 1);
							return 0;
						}
						$numKeys->arr[$i] = $item->internalValue;
						break;
					case 5:
						if (!$isString) {
							self::PST_assignIndexHack((self::PST_intBuffer16), 0, 1);
							return 0;
						}
						$stringKeys->arr[$i] = $item->internalValue;
						break;
					default:
						self::PST_assignIndexHack((self::PST_intBuffer16), 0, 1);
						return 0;
				}
				$i += 1;
			}
			self::qsortHelper($stringKeys, $numKeys, $indices, $isString, 0, ($length - 1));
			$i = 0;
			while (($i < $length)) {
				self::PST_assignIndexHack($parallelList->list, $i, $originalOrder->arr[$indices->arr[$i]]);
				$i += 1;
			}
			return 0;
		}

		public static function stackItemIsLibrary($stackInfo) {
			if (($stackInfo[0] != "[")) {
				return false;
			}
			$cIndex = self::PST_stringIndexOf($stackInfo, ":", 0);
			return (($cIndex > 0) && ($cIndex < self::PST_stringIndexOf($stackInfo, "]", 0)));
		}

		public static function startVm($vm) {
			return self::runInterpreter($vm, $vm->lastExecutionContextId);
		}

		public static function stringDecode($encoded) {
			if (!(strpos($encoded, "%") !== false)) {
				$length = strlen($encoded);
				$per = "%";
				$builder = new PastelPtrArray();
				$i = 0;
				while (($i < $length)) {
					$c = $encoded[$i];
					if ((($c == $per) && (($i + 2) < $length))) {
						array_push($builder->arr, self::stringFromHex(implode(array("", $encoded[($i + 1)], $encoded[($i + 2)]))));
					} else {
						array_push($builder->arr, (("") . ($c)));
					}
					$i += 1;
				}
				return implode("", $builder->arr);
			}
			return $encoded;
		}

		public static function stringFromHex($encoded) {
			$encoded = strtoupper($encoded);
			$hex = "0123456789ABCDEF";
			$output = new PastelPtrArray();
			$length = strlen($encoded);
			$a = 0;
			$b = 0;
			$c = null;
			$i = 0;
			while ((($i + 1) < $length)) {
				$c = (("") . ($encoded[$i]));
				$a = self::PST_stringIndexOf($hex, $c, 0);
				if (($a == -1)) {
					return null;
				}
				$c = (("") . ($encoded[($i + 1)]));
				$b = self::PST_stringIndexOf($hex, $c, 0);
				if (($b == -1)) {
					return null;
				}
				$a = (($a * 16) + $b);
				array_push($output->arr, chr($a));
				$i += 2;
			}
			return implode("", $output->arr);
		}

		public static function suspendInterpreter() {
			return new InterpreterResult(2, null, 0.0, 0, false, "");
		}

		public static function tokenDataImpl($vm, $row) {
			$tokensByPc = $vm->symbolData->tokenData;
			$pc = ($row->arr[0] + $vm->metadata->userCodeStart);
			$line = $row->arr[1];
			$col = $row->arr[2];
			$file = $row->arr[3];
			$tokens = $tokensByPc->arr[$pc];
			if (($tokens == null)) {
				$tokens = new PastelPtrArray();
				$tokensByPc->arr[$pc] = $tokens;
			}
			array_push($tokens->arr, new Token($line, $col, $file));
			return 0;
		}

		public static function tokenHelperConvertPcsToStackTraceStrings($vm, $pcs) {
			$tokens = self::generateTokenListFromPcs($vm, $pcs);
			$files = $vm->symbolData->sourceCode;
			$output = new PastelPtrArray();
			$i = 0;
			while (($i < count($tokens->arr))) {
				$token = $tokens->arr[$i];
				if (($token == null)) {
					array_push($output->arr, "[No stack information]");
				} else {
					$line = $token->lineIndex;
					$col = $token->colIndex;
					$fileData = $files->arr[$token->fileId];
					$lines = pastelWrapList(explode("\n", $fileData));
					$filename = $lines->arr[0];
					$linevalue = $lines->arr[($line + 1)];
					array_push($output->arr, implode(array($filename, ", Line: ", ('' . (($line + 1))), ", Col: ", ('' . (($col + 1))))));
				}
				$i += 1;
			}
			return $output;
		}

		public static function tokenHelperGetFileLine($vm, $fileId, $lineNum) {
			$sourceCode = $vm->symbolData->sourceCode->arr[$fileId];
			if (($sourceCode == null)) {
				return null;
			}
			return pastelWrapList(explode("\n", $sourceCode))->arr[$lineNum];
		}

		public static function tokenHelperGetFormattedPointerToToken($vm, $token) {
			$line = self::tokenHelperGetFileLine($vm, $token->fileId, ($token->lineIndex + 1));
			if (($line == null)) {
				return null;
			}
			$columnIndex = $token->colIndex;
			$lineLength = strlen($line);
			$line = ltrim($line);
			$line = str_replace("\t", " ", $line);
			$offset = ($lineLength - strlen($line));
			$columnIndex -= $offset;
			$line2 = "";
			while (($columnIndex > 0)) {
				$columnIndex -= 1;
				$line2 = (($line2) . (" "));
			}
			$line2 = (($line2) . ("^"));
			return implode(array($line, "\n", $line2));
		}

		public static function tokenHelplerIsFilePathLibrary($vm, $fileId, $allFiles) {
			$filename = self::tokenHelperGetFileLine($vm, $fileId, 0);
			return !self::PST_stringEndsWith(strtolower($filename), ".cry");
		}

		public static function typeInfoToString($vm, $typeInfo, $i) {
			$output = new PastelPtrArray();
			self::typeToStringBuilder($vm, $output, $typeInfo, $i);
			return implode("", $output->arr);
		}

		public static function typeToString($vm, $typeInfo, $i) {
			$sb = new PastelPtrArray();
			self::typeToStringBuilder($vm, $sb, $typeInfo, $i);
			return implode("", $sb->arr);
		}

		public static function typeToStringBuilder($vm, $sb, $typeInfo, $i) {
			switch ($typeInfo->arr[$i]) {
				case -1:
					array_push($sb->arr, "void");
					return ($i + 1);
				case 0:
					array_push($sb->arr, "object");
					return ($i + 1);
				case 1:
					array_push($sb->arr, "object");
					return ($i + 1);
				case 3:
					array_push($sb->arr, "int");
					return ($i + 1);
				case 4:
					array_push($sb->arr, "float");
					return ($i + 1);
				case 2:
					array_push($sb->arr, "bool");
					return ($i + 1);
				case 5:
					array_push($sb->arr, "string");
					return ($i + 1);
				case 6:
					array_push($sb->arr, "List<");
					$i = self::typeToStringBuilder($vm, $sb, $typeInfo, ($i + 1));
					array_push($sb->arr, ">");
					return $i;
				case 7:
					array_push($sb->arr, "Dictionary<");
					$i = self::typeToStringBuilder($vm, $sb, $typeInfo, ($i + 1));
					array_push($sb->arr, ", ");
					$i = self::typeToStringBuilder($vm, $sb, $typeInfo, $i);
					array_push($sb->arr, ">");
					return $i;
				case 8:
					$classId = $typeInfo->arr[($i + 1)];
					if (($classId == 0)) {
						array_push($sb->arr, "object");
					} else {
						$classInfo = $vm->metadata->classTable->arr[$classId];
						array_push($sb->arr, $classInfo->fullyQualifiedName);
					}
					return ($i + 2);
				case 10:
					array_push($sb->arr, "Class");
					return ($i + 1);
				case 9:
					$n = $typeInfo->arr[($i + 1)];
					$optCount = $typeInfo->arr[($i + 2)];
					$i += 2;
					array_push($sb->arr, "function(");
					$ret = new PastelPtrArray();
					$i = self::typeToStringBuilder($vm, $ret, $typeInfo, $i);
					$j = 1;
					while (($j < $n)) {
						if (($j > 1)) {
							array_push($sb->arr, ", ");
						}
						$i = self::typeToStringBuilder($vm, $sb, $typeInfo, $i);
						$j += 1;
					}
					if (($n == 1)) {
						array_push($sb->arr, "void");
					}
					array_push($sb->arr, " => ");
					$optStart = ($n - $optCount - 1);
					$j = 0;
					while (($j < count($ret->arr))) {
						if (($j >= $optStart)) {
							array_push($sb->arr, "(opt) ");
						}
						array_push($sb->arr, $ret->arr[$j]);
						$j += 1;
					}
					array_push($sb->arr, ")");
					return $i;
				default:
					array_push($sb->arr, "UNKNOWN");
					return ($i + 1);
			}
		}

		public static function typeToStringFromValue($vm, $value) {
			$sb = null;
			switch ($value->type) {
				case 1:
					return "null";
				case 2:
					return "bool";
				case 3:
					return "int";
				case 4:
					return "float";
				case 5:
					return "string";
				case 10:
					return "class";
				case 8:
					$classId = ($value->internalValue)->classId;
					$classInfo = $vm->metadata->classTable->arr[$classId];
					return $classInfo->fullyQualifiedName;
				case 6:
					$sb = new PastelPtrArray();
					array_push($sb->arr, "List<");
					$list = $value->internalValue;
					if (($list->type == null)) {
						array_push($sb->arr, "object");
					} else {
						self::typeToStringBuilder($vm, $sb, $list->type, 0);
					}
					array_push($sb->arr, ">");
					return implode("", $sb->arr);
				case 7:
					$dict = $value->internalValue;
					$sb = new PastelPtrArray();
					array_push($sb->arr, "Dictionary<");
					switch ($dict->keyType) {
						case 3:
							array_push($sb->arr, "int");
							break;
						case 5:
							array_push($sb->arr, "string");
							break;
						case 8:
							array_push($sb->arr, "object");
							break;
						default:
							array_push($sb->arr, "???");
							break;
					}
					array_push($sb->arr, ", ");
					if (($dict->valueType == null)) {
						array_push($sb->arr, "object");
					} else {
						self::typeToStringBuilder($vm, $sb, $dict->valueType, 0);
					}
					array_push($sb->arr, ">");
					return implode("", $sb->arr);
				case 9:
					return "Function";
				default:
					return "Unknown";
			}
		}

		public static function uncaughtExceptionResult($vm, $exception) {
			return new InterpreterResult(3, self::unrollExceptionOutput($vm, $exception), 0.0, 0, false, "");
		}

		public static function unrollExceptionOutput($vm, $exceptionInstance) {
			$objInstance = $exceptionInstance->internalValue;
			$classInfo = $vm->metadata->classTable->arr[$objInstance->classId];
			$pcs = $objInstance->nativeData->arr[1];
			$codeFormattedPointer = "";
			$exceptionName = $classInfo->fullyQualifiedName;
			$message = self::valueToString($vm, $objInstance->members->arr[1]);
			$trace = self::tokenHelperConvertPcsToStackTraceStrings($vm, $pcs);
			array_pop($trace->arr);
			array_push($trace->arr, "Stack Trace:");
			self::PST_reverseArray($trace);
			self::PST_reverseArray($pcs);
			$showLibStack = $vm->environment->showLibStack;
			if ((!$showLibStack && !self::stackItemIsLibrary($trace->arr[0]))) {
				while (self::stackItemIsLibrary($trace->arr[(count($trace->arr) - 1)])) {
					array_pop($trace->arr);
					array_pop($pcs->arr);
				}
			}
			$tokensAtPc = $vm->symbolData->tokenData->arr[$pcs->arr[(count($pcs->arr) - 1)]];
			if (($tokensAtPc != null)) {
				$codeFormattedPointer = (("\n\n") . (self::tokenHelperGetFormattedPointerToToken($vm, $tokensAtPc->arr[0])));
			}
			$stackTrace = implode("\n", $trace->arr);
			return implode(array($stackTrace, $codeFormattedPointer, "\n", $exceptionName, ": ", $message));
		}

		public static function valueConcatLists($a, $b) {
			return new ListImpl(null, ($a->size + $b->size), pastelWrapList(array_merge($a->list->arr, $b->list->arr)));
		}

		public static function valueMultiplyList($a, $n) {
			$_len = ($a->size * $n);
			$output = self::makeEmptyList($a->type, $_len);
			if (($_len == 0)) {
				return $output;
			}
			$aLen = $a->size;
			$i = 0;
			$value = null;
			if (($aLen == 1)) {
				$value = $a->list->arr[0];
				$i = 0;
				while (($i < $n)) {
					array_push($output->list->arr, $value);
					$i += 1;
				}
			} else {
				$j = 0;
				$i = 0;
				while (($i < $n)) {
					$j = 0;
					while (($j < $aLen)) {
						array_push($output->list->arr, $a->list->arr[$j]);
						$j += 1;
					}
					$i += 1;
				}
			}
			$output->size = $_len;
			return $output;
		}

		public static function valueStackIncreaseCapacity($ec) {
			$stack = $ec->valueStack;
			$oldCapacity = count($stack->arr);
			$newCapacity = ($oldCapacity * 2);
			$newStack = pastelWrapList(array_fill(0, $newCapacity, null));
			$i = ($oldCapacity - 1);
			while (($i >= 0)) {
				$newStack->arr[$i] = $stack->arr[$i];
				$i -= 1;
			}
			$ec->valueStack = $newStack;
			return $newStack;
		}

		public static function valueToString($vm, $wrappedValue) {
			$type = $wrappedValue->type;
			if (($type == 1)) {
				return "null";
			}
			if (($type == 2)) {
				if ($wrappedValue->internalValue) {
					return "true";
				}
				return "false";
			}
			if (($type == 4)) {
				$floatStr = ('' . ($wrappedValue->internalValue));
				if (!(strpos($floatStr, ".") !== false)) {
					$floatStr .= ".0";
				}
				return $floatStr;
			}
			if (($type == 3)) {
				return ('' . ($wrappedValue->internalValue));
			}
			if (($type == 5)) {
				return $wrappedValue->internalValue;
			}
			if (($type == 6)) {
				$internalList = $wrappedValue->internalValue;
				$output = "[";
				$i = 0;
				while (($i < $internalList->size)) {
					if (($i > 0)) {
						$output .= ", ";
					}
					$output .= self::valueToString($vm, $internalList->list->arr[$i]);
					$i += 1;
				}
				$output .= "]";
				return $output;
			}
			if (($type == 8)) {
				$objInstance = $wrappedValue->internalValue;
				$classId = $objInstance->classId;
				$ptr = $objInstance->objectId;
				$classInfo = $vm->metadata->classTable->arr[$classId];
				$nameId = $classInfo->nameId;
				$className = $vm->metadata->identifiers->arr[$nameId];
				return implode(array("Instance<", $className, "#", ('' . ($ptr)), ">"));
			}
			if (($type == 7)) {
				$dict = $wrappedValue->internalValue;
				if (($dict->size == 0)) {
					return "{}";
				}
				$output = "{";
				$keyList = $dict->keys;
				$valueList = $dict->values;
				$i = 0;
				while (($i < $dict->size)) {
					if (($i > 0)) {
						$output .= ", ";
					}
					$output .= implode(array(self::valueToString($vm, $dict->keys->arr[$i]), ": ", self::valueToString($vm, $dict->values->arr[$i])));
					$i += 1;
				}
				$output .= " }";
				return $output;
			}
			if (($type == 9)) {
				$fp = $wrappedValue->internalValue;
				switch ($fp->type) {
					case 1:
						return "<FunctionPointer>";
					case 2:
						return "<ClassMethodPointer>";
					case 3:
						return "<ClassStaticMethodPointer>";
					case 4:
						return "<PrimitiveMethodPointer>";
					case 5:
						return "<Lambda>";
					default:
						return "<UnknownFunctionPointer>";
				}
			}
			return "<unknown>";
		}

		public static function vm_getCurrentExecutionContextId($vm) {
			return $vm->lastExecutionContextId;
		}

		public static function vm_suspend_context_by_id($vm, $execId, $status) {
			return self::vm_suspend_for_context(self::getExecutionContext($vm, $execId), 1);
		}

		public static function vm_suspend_for_context($ec, $status) {
			$ec->executionStateChange = true;
			$ec->executionStateChangeCommand = $status;
			return 0;
		}

		public static function vm_suspend_with_status_by_id($vm, $execId, $status) {
			return self::vm_suspend_for_context(self::getExecutionContext($vm, $execId), $status);
		}

		public static function vmEnvSetCommandLineArgs($vm, $args) {
			$vm->environment->commandLineArgs = $args;
		}

		public static function vmGetGlobals($vm) {
			return $vm->globals;
		}
	}
	PastelGeneratedCode::$PST_intBuffer16 = pastelWrapList(array_fill(0, 16, 0));

?>