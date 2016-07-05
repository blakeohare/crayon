<?php
  $nullhack = null;
  
  function &array_hack($array) { return $array; }
  
  function pth_noop() { }
  
	function &pth_new_array($size) {
    $output = array();
    while ($size-- > 0) array_push($output, null);
    return $output;
  }
  
  function &pth_dictionary_get_keys(&$dictionary) {
    $output = array();
    foreach ($dictionary as $key => $ignored) {
      array_push($output, $key);
    }
    return $output;
  }
  
  $pth_program_data = null;
  function pth_setProgramData(&$pd) {
    $GLOBALS['pth_program_data'] = &$pd;
  }
  
  function &pth_getProgramData() {
    return $GLOBALS['pth_program_data'];
  }
  
  function pth_list_reverse(&$list) {
    $length = count($list);
    for ($i = intval($length / 2) - 1; $i >= 0; --$i) {
      $j = $length - $i - 1;
      $refLeft = is_array($list[$i]);
      $refRight = is_array($list[$j]);
      if ($refLeft) $t = &$list[$i]; else $t = $list[$i];
      if ($refRight) $list[$i] = &$list[$j]; else $list[$i] = $list[$j];
      if ($refLeft) $list[$j] = &$t; else $list[$j] = $t;
    }
  }
  
  function pth_getHttpRequest(&$stringOut) {
    $stringOut[0] = $_SERVER['REQUEST_METHOD'];
    $stringOut[1] = $_SERVER['REQUEST_URI'];
    $stringOut[2] = $_SERVER['REMOTE_HOST'];
  }
?>